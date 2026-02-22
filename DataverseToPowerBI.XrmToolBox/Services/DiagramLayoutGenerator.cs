// ===================================================================================
// DiagramLayoutGenerator.cs - Power BI Model Diagram Layout Generation
// ===================================================================================
//
// PURPOSE:
// Generates diagramLayout.json for PBIP projects so that the Power BI Desktop
// Model View opens with a clean, relationship-aware arrangement of table cards
// instead of the default overlapping pile.
//
// LAYOUT ALGORITHMS:
// - Star layout (single fact table): Fact at center, dimensions in a ring,
//   snowflake children pushed outward behind their parent dimension.
// - Grid layout (multiple fact tables): Facts stacked vertically on the left,
//   dimensions in a horizontal row below, snowflake groups at the end.
//
// INTEGRATION:
// Called at the end of Build()/BuildIncremental() after all tables and
// relationships are finalized. Writes diagramLayout.json alongside other
// TMDL definition files.
//
// ===================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using DataverseToPowerBI.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataverseToPowerBI.XrmToolBox.Services
{
    /// <summary>
    /// Generates Power BI diagram layout JSON for arranging table cards in Model View.
    /// </summary>
    internal static class DiagramLayoutGenerator
    {
        #region Constants

        private const double DEFAULT_TABLE_WIDTH = 234;
        private const double HEADER_HEIGHT = 45;
        private const double ROW_HEIGHT_PER_COLUMN = 20;
        private const double MIN_TABLE_HEIGHT = 150;
        private const double MAX_TABLE_HEIGHT = 600;
        private const double COLLAPSED_TABLE_HEIGHT = 50;

        private const double STAR_RADIUS = 350;
        private const double SNOWFLAKE_PUSH = 280;
        private const double COLUMN_GAP = 40;
        private const double ROW_GAP = 30;
        private const double MARGIN = 50;

        #endregion

        #region Public API

        /// <summary>
        /// Generates the diagramLayout.json content for a set of tables and relationships.
        /// Tables are arranged using a star layout (single fact) or grid layout (multiple facts).
        /// </summary>
        /// <param name="tables">All tables in the semantic model.</param>
        /// <param name="relationships">All relationships between tables.</param>
        /// <param name="dateTableConfig">Optional date table config (adds a "Date" node).</param>
        /// <returns>JSON string for diagramLayout.json.</returns>
        public static string Generate(
            List<ExportTable> tables,
            List<ExportRelationship> relationships,
            DateTableConfig? dateTableConfig = null)
        {
            if (tables == null || tables.Count == 0)
                return GenerateEmptyLayout();

            // Build table name list (display names as used in TMDL)
            var tableNames = new List<string>();
            var columnCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var tableRoles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var table in tables)
            {
                var name = table.DisplayName ?? table.SchemaName ?? table.LogicalName;
                tableNames.Add(name);
                columnCounts[name] = table.Attributes?.Count ?? 5;
                tableRoles[name] = table.Role ?? "Dimension";
            }

            // Add special tables
            tableNames.Add("DataverseURL");
            columnCounts["DataverseURL"] = 1;
            tableRoles["DataverseURL"] = "Other";

            if (dateTableConfig != null)
            {
                tableNames.Add("Date");
                columnCounts["Date"] = 15; // Date tables typically have ~15 columns
                tableRoles["Date"] = "Dimension";
            }

            // Classify tables
            var factTables = new List<string>();
            var dimTables = new List<string>();
            var otherTables = new List<string>();

            foreach (var name in tableNames)
            {
                string role;
                if (!tableRoles.TryGetValue(name, out role))
                    role = "Dimension";

                if (role.Equals("Fact", StringComparison.OrdinalIgnoreCase))
                    factTables.Add(name);
                else if (role.Equals("Other", StringComparison.OrdinalIgnoreCase))
                    otherTables.Add(name);
                else
                    dimTables.Add(name);
            }

            // Build adjacency from relationships
            BuildAdjacency(relationships, tables, factTables, dimTables,
                out var factToDims, out var snowflake, out var orphanDims);

            // Compute node sizes (dimensions use collapsed height for layout spacing)
            var nodeSizes = new Dictionary<string, double[]>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in tableNames)
            {
                int cols;
                if (!columnCounts.TryGetValue(name, out cols))
                    cols = 5;

                string role;
                if (!tableRoles.TryGetValue(name, out role))
                    role = "Dimension";

                // Dimensions will be collapsed — use collapsed height for layout spacing
                bool isDimension = role.Equals("Dimension", StringComparison.OrdinalIgnoreCase);
                double layoutHeight = isDimension ? COLLAPSED_TABLE_HEIGHT : EstimateCardHeight(cols);
                double fullHeight = EstimateCardHeight(cols);
                // Store: [width, layoutHeight, fullHeight]
                nodeSizes[name] = new[] { DEFAULT_TABLE_WIDTH, layoutHeight, fullHeight };
            }

            // Compute positions
            var positions = ComputeLayout(
                factTables, dimTables, otherTables,
                factToDims, snowflake, orphanDims,
                nodeSizes);

            // Shift to positive space with margin
            NormalizePositions(positions);

            // Build JSON
            return BuildLayoutJson(tableNames, positions, nodeSizes, tableRoles);
        }

        #endregion

        #region Adjacency Building

        /// <summary>
        /// Builds relationship graph: fact→dim links and snowflake (dim→dim) links.
        /// Uses ExportRelationship metadata which already has SourceTable/TargetTable.
        /// </summary>
        private static void BuildAdjacency(
            List<ExportRelationship> relationships,
            List<ExportTable> tables,
            List<string> factTables,
            List<string> dimTables,
            out Dictionary<string, List<string>> factToDims,
            out Dictionary<string, List<string>> snowflake,
            out HashSet<string> orphanDims)
        {
            var factSet = new HashSet<string>(factTables, StringComparer.OrdinalIgnoreCase);
            var dimSet = new HashSet<string>(dimTables, StringComparer.OrdinalIgnoreCase);

            // Map logical names to display names for relationship resolution
            var logicalToDisplay = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var table in tables)
            {
                var display = table.DisplayName ?? table.SchemaName ?? table.LogicalName;
                logicalToDisplay[table.LogicalName] = display;
            }

            factToDims = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            snowflake = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var linkedDims = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var rel in relationships)
            {
                string sourceName;
                if (!logicalToDisplay.TryGetValue(rel.SourceTable, out sourceName))
                    sourceName = rel.SourceTable;

                string targetName;
                if (!logicalToDisplay.TryGetValue(rel.TargetTable, out targetName))
                    targetName = rel.TargetTable;

                if (factSet.Contains(sourceName) && dimSet.Contains(targetName))
                {
                    // Fact → Dim (star link)
                    if (!factToDims.ContainsKey(sourceName))
                        factToDims[sourceName] = new List<string>();
                    if (!factToDims[sourceName].Any(d => d.Equals(targetName, StringComparison.OrdinalIgnoreCase)))
                        factToDims[sourceName].Add(targetName);
                    linkedDims.Add(targetName);
                }
                else if (dimSet.Contains(sourceName) && dimSet.Contains(targetName))
                {
                    // Dim → Dim (snowflake link)
                    if (!snowflake.ContainsKey(sourceName))
                        snowflake[sourceName] = new List<string>();
                    if (!snowflake[sourceName].Any(d => d.Equals(targetName, StringComparison.OrdinalIgnoreCase)))
                        snowflake[sourceName].Add(targetName);
                    linkedDims.Add(sourceName);
                    linkedDims.Add(targetName);
                }
                else if (factSet.Contains(targetName) && dimSet.Contains(sourceName))
                {
                    // Reversed: Dim is source, Fact is target
                    if (!factToDims.ContainsKey(targetName))
                        factToDims[targetName] = new List<string>();
                    if (!factToDims[targetName].Any(d => d.Equals(sourceName, StringComparison.OrdinalIgnoreCase)))
                        factToDims[targetName].Add(sourceName);
                    linkedDims.Add(sourceName);
                }
            }

            // Date table is always linked to all facts via date relationships
            if (dimSet.Contains("Date"))
            {
                linkedDims.Add("Date");
                foreach (var fact in factTables)
                {
                    if (!factToDims.ContainsKey(fact))
                        factToDims[fact] = new List<string>();
                    if (!factToDims[fact].Any(d => d.Equals("Date", StringComparison.OrdinalIgnoreCase)))
                        factToDims[fact].Add("Date");
                }
            }

            orphanDims = new HashSet<string>(
                dimSet.Where(d => !linkedDims.Contains(d)),
                StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region Layout Computation

        /// <summary>
        /// Computes table positions using star layout (single fact) or grid layout (multiple facts).
        /// </summary>
        private static Dictionary<string, double[]> ComputeLayout(
            List<string> factTables,
            List<string> dimTables,
            List<string> otherTables,
            Dictionary<string, List<string>> factToDims,
            Dictionary<string, List<string>> snowflake,
            HashSet<string> orphanDims,
            Dictionary<string, double[]> nodeSizes)
        {
            // positions: key → [x, y]
            var positions = new Dictionary<string, double[]>(StringComparer.OrdinalIgnoreCase);

            // Collect all snowflake children
            var allSnowflakeChildren = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var children in snowflake.Values)
                allSnowflakeChildren.UnionWith(children);

            // Sort facts by relationship count (most connections first)
            var sortedFacts = factTables
                .OrderByDescending(f =>
                {
                    List<string> dims;
                    return factToDims.TryGetValue(f, out dims) ? dims.Count : 0;
                })
                .ToList();

            if (sortedFacts.Count == 1)
            {
                ComputeStarLayout(sortedFacts[0], dimTables, factToDims, snowflake,
                    allSnowflakeChildren, nodeSizes, positions);
            }
            else if (sortedFacts.Count > 1)
            {
                ComputeGridLayout(sortedFacts, dimTables, factToDims, snowflake,
                    allSnowflakeChildren, nodeSizes, positions);
            }
            else
            {
                // No facts — arrange all tables in a simple grid
                ComputeSimpleGrid(dimTables.Concat(otherTables).ToList(), nodeSizes, positions);
            }

            // Place any remaining unplaced tables (other/utility tables)
            PlaceUnplacedTables(otherTables, nodeSizes, positions);

            return positions;
        }

        /// <summary>
        /// Star layout: fact at center, dimensions in a ring, snowflake children pushed outward.
        /// </summary>
        private static void ComputeStarLayout(
            string factTable,
            List<string> dimTables,
            Dictionary<string, List<string>> factToDims,
            Dictionary<string, List<string>> snowflake,
            HashSet<string> allSnowflakeChildren,
            Dictionary<string, double[]> nodeSizes,
            Dictionary<string, double[]> positions)
        {
            // Fact at center
            positions[factTable] = new[] { 0.0, 0.0 };

            // Ring dims = direct dims excluding snowflake children
            List<string> dims;
            if (!factToDims.TryGetValue(factTable, out dims))
                dims = new List<string>();
            var ringDims = dims.Where(d => !allSnowflakeChildren.Contains(d)).ToList();

            int n = ringDims.Count;
            for (int i = 0; i < n; i++)
            {
                var dim = ringDims[i];
                var angle = ToRadians(-90.0 + (360.0 * i / Math.Max(n, 1)));
                var size = GetNodeSize(nodeSizes, dim);
                double x = STAR_RADIUS * Math.Cos(angle) - size[0] / 2;
                double y = STAR_RADIUS * Math.Sin(angle) - size[1] / 2;
                positions[dim] = new[] { x, y };

                // Place snowflake children behind parent
                List<string> children;
                if (snowflake.TryGetValue(dim, out children))
                {
                    for (int j = 0; j < children.Count; j++)
                    {
                        var child = children[j];
                        var push = SNOWFLAKE_PUSH * (j + 1);
                        var childSize = GetNodeSize(nodeSizes, child);
                        double cx = (STAR_RADIUS + push) * Math.Cos(angle) - childSize[0] / 2;
                        double cy = (STAR_RADIUS + push) * Math.Sin(angle) - childSize[1] / 2;
                        positions[child] = new[] { cx, cy };
                    }
                }
            }

            // Place orphan dims in a row below
            var unplaced = dimTables.Where(d => !positions.ContainsKey(d)).ToList();
            if (unplaced.Count > 0)
            {
                double maxBottom = 0;
                foreach (var kv in positions)
                {
                    var ns = GetNodeSize(nodeSizes, kv.Key);
                    double bottom = kv.Value[1] + ns[1];
                    if (bottom > maxBottom) maxBottom = bottom;
                }
                double xCursor = -((unplaced.Count - 1) * (DEFAULT_TABLE_WIDTH + COLUMN_GAP)) / 2;
                foreach (var dim in unplaced)
                {
                    var size = GetNodeSize(nodeSizes, dim);
                    positions[dim] = new[] { xCursor, maxBottom + ROW_GAP };
                    xCursor += size[0] + COLUMN_GAP;
                }
            }
        }

        /// <summary>
        /// Grid layout: facts stacked left, dimensions in a horizontal row below.
        /// </summary>
        private static void ComputeGridLayout(
            List<string> factTables,
            List<string> dimTables,
            Dictionary<string, List<string>> factToDims,
            Dictionary<string, List<string>> snowflake,
            HashSet<string> allSnowflakeChildren,
            Dictionary<string, double[]> nodeSizes,
            Dictionary<string, double[]> positions)
        {
            // Calculate fact column width
            double factColWidth = factTables.Max(f => GetNodeSize(nodeSizes, f)[0]);

            // Stack facts vertically
            double yCursor = 0;
            foreach (var fact in factTables)
            {
                positions[fact] = new[] { 0.0, yCursor };
                yCursor += GetNodeSize(nodeSizes, fact)[1] + ROW_GAP;
            }

            double factBlockBottom = yCursor;

            // Build dim row: plain dims first, then snowflake groups
            var snowflakeParents = new HashSet<string>(snowflake.Keys, StringComparer.OrdinalIgnoreCase);
            var plainDims = dimTables
                .Where(d => !allSnowflakeChildren.Contains(d) && !snowflakeParents.Contains(d))
                .ToList();

            var tail = new List<string>();
            foreach (var d in dimTables)
            {
                if (snowflakeParents.Contains(d))
                {
                    tail.Add(d);
                    List<string> children;
                    if (snowflake.TryGetValue(d, out children))
                        tail.AddRange(children);
                }
            }

            var dimRow = plainDims.Concat(tail).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            // Place dims in horizontal row below facts
            double xCursor = factColWidth + COLUMN_GAP;
            foreach (var name in dimRow)
            {
                positions[name] = new[] { xCursor, factBlockBottom };
                xCursor += GetNodeSize(nodeSizes, name)[0] + COLUMN_GAP;
            }

            // Place unplaced dims (orphans)
            var unplacedDims = dimTables.Where(d => !positions.ContainsKey(d)).ToList();
            foreach (var name in unplacedDims)
            {
                positions[name] = new[] { xCursor, factBlockBottom };
                xCursor += GetNodeSize(nodeSizes, name)[0] + COLUMN_GAP;
            }
        }

        /// <summary>
        /// Simple grid for when there are no fact tables.
        /// </summary>
        private static void ComputeSimpleGrid(
            List<string> tableNames,
            Dictionary<string, double[]> nodeSizes,
            Dictionary<string, double[]> positions)
        {
            int cols = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(tableNames.Count)));
            double xCursor = 0;
            double yCursor = 0;
            double rowMaxHeight = 0;
            int col = 0;

            foreach (var name in tableNames)
            {
                var size = GetNodeSize(nodeSizes, name);
                positions[name] = new[] { xCursor, yCursor };
                rowMaxHeight = Math.Max(rowMaxHeight, size[1]);
                xCursor += size[0] + COLUMN_GAP;
                col++;

                if (col >= cols)
                {
                    col = 0;
                    xCursor = 0;
                    yCursor += rowMaxHeight + ROW_GAP;
                    rowMaxHeight = 0;
                }
            }
        }

        /// <summary>
        /// Places any remaining tables that weren't positioned by the main layout.
        /// </summary>
        private static void PlaceUnplacedTables(
            List<string> otherTables,
            Dictionary<string, double[]> nodeSizes,
            Dictionary<string, double[]> positions)
        {
            var unplaced = otherTables.Where(t => !positions.ContainsKey(t)).ToList();
            if (unplaced.Count == 0) return;

            // Place below everything else, using actual node heights for gap calculation
            double maxBottom = 0;
            foreach (var kv in positions)
            {
                var nodeSize = GetNodeSize(nodeSizes, kv.Key);
                double bottom = kv.Value[1] + nodeSize[1];
                if (bottom > maxBottom) maxBottom = bottom;
            }

            double xCursor = 0;
            foreach (var name in unplaced)
            {
                positions[name] = new[] { xCursor, maxBottom + ROW_GAP };
                xCursor += GetNodeSize(nodeSizes, name)[0] + COLUMN_GAP;
            }
        }

        #endregion

        #region JSON Generation

        /// <summary>
        /// Shifts all positions so the top-left table is at (MARGIN, MARGIN).
        /// </summary>
        private static void NormalizePositions(Dictionary<string, double[]> positions)
        {
            if (positions.Count == 0) return;

            double minX = positions.Values.Min(p => p[0]);
            double minY = positions.Values.Min(p => p[1]);
            double offsetX = MARGIN - minX;
            double offsetY = MARGIN - minY;

            foreach (var key in positions.Keys.ToList())
            {
                var pos = positions[key];
                positions[key] = new[] { Math.Round(pos[0] + offsetX, 1), Math.Round(pos[1] + offsetY, 1) };
            }
        }

        /// <summary>
        /// Builds the diagramLayout.json structure.
        /// Dimension tables are collapsed; pinKeyFieldsToTop is enabled.
        /// </summary>
        private static string BuildLayoutJson(
            List<string> tableNames,
            Dictionary<string, double[]> positions,
            Dictionary<string, double[]> nodeSizes,
            Dictionary<string, string> tableRoles)
        {
            var nodes = new JArray();
            int zIndex = 0;

            foreach (var name in tableNames)
            {
                if (!positions.ContainsKey(name)) continue;

                var pos = positions[name];
                var size = GetNodeSize(nodeSizes, name);
                // size[0]=width, size[1]=layoutHeight, size[2]=fullHeight (if present)
                double fullHeight = size.Length > 2 ? size[2] : size[1];

                string role;
                if (!tableRoles.TryGetValue(name, out role))
                    role = "Dimension";
                bool isDimension = role.Equals("Dimension", StringComparison.OrdinalIgnoreCase);

                var node = new JObject
                {
                    ["location"] = new JObject
                    {
                        ["x"] = pos[0],
                        ["y"] = pos[1]
                    },
                    ["nodeIndex"] = name,
                    ["size"] = new JObject
                    {
                        ["height"] = Math.Round(fullHeight, 1),
                        ["width"] = Math.Round(size[0], 1)
                    },
                    ["zIndex"] = zIndex++
                };

                // Collapse dimension tables for a cleaner Model View
                if (isDimension)
                {
                    node["isCollapsed"] = true;
                }

                nodes.Add(node);
            }

            var layout = new JObject
            {
                ["version"] = "1.1.0",
                ["diagrams"] = new JArray
                {
                    new JObject
                    {
                        ["ordinal"] = 0,
                        ["scrollPosition"] = new JObject { ["x"] = 0, ["y"] = 0 },
                        ["nodes"] = nodes,
                        ["name"] = "All tables",
                        ["zoomValue"] = 100,
                        ["pinKeyFieldsToTop"] = true,
                        ["showExtraHeaderInfo"] = false,
                        ["hideKeyFieldsWhenCollapsed"] = false,
                        ["tablesLocked"] = false
                    }
                },
                ["selectedDiagram"] = "All tables",
                ["defaultDiagram"] = "All tables"
            };

            return layout.ToString(Formatting.Indented);
        }

        /// <summary>
        /// Generates an empty layout with no nodes.
        /// </summary>
        private static string GenerateEmptyLayout()
        {
            var layout = new JObject
            {
                ["version"] = "1.1.0",
                ["diagrams"] = new JArray
                {
                    new JObject
                    {
                        ["ordinal"] = 0,
                        ["scrollPosition"] = new JObject { ["x"] = 0, ["y"] = 0 },
                        ["nodes"] = new JArray(),
                        ["name"] = "All tables",
                        ["zoomValue"] = 100
                    }
                },
                ["selectedDiagram"] = "All tables",
                ["defaultDiagram"] = "All tables"
            };

            return layout.ToString(Formatting.Indented);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Gets node size (width, height) from the sizes dictionary with a default fallback.
        /// Returns double[] where [0]=width, [1]=height.
        /// </summary>
        private static double[] GetNodeSize(Dictionary<string, double[]> nodeSizes, string name)
        {
            double[] size;
            if (nodeSizes.TryGetValue(name, out size))
                return size;
            return new[] { DEFAULT_TABLE_WIDTH, MIN_TABLE_HEIGHT };
        }

        /// <summary>
        /// Estimates Power BI card height from column count.
        /// Power BI scales card height based on the number of visible columns.
        /// </summary>
        private static double EstimateCardHeight(int columnCount)
        {
            double height = HEADER_HEIGHT + (columnCount * ROW_HEIGHT_PER_COLUMN);
            return Math.Max(MIN_TABLE_HEIGHT, Math.Min(height, MAX_TABLE_HEIGHT));
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

        #endregion
    }
}
