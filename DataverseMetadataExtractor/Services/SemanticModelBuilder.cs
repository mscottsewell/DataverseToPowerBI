using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DataverseMetadataExtractor.Models;
using DataverseMetadataExtractor.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataverseMetadataExtractor.Services
{
    /// <summary>
    /// Builds a Power BI Semantic Model (PBIP) from Dataverse metadata
    /// </summary>
    public class SemanticModelBuilder
    {
        private readonly string _templatePath;
        private readonly Action<string>? _statusCallback;
        private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(false);

        public SemanticModelBuilder(string templatePath, Action<string>? statusCallback = null)
        {
            _templatePath = templatePath;
            _statusCallback = statusCallback;
        }

        private void SetStatus(string message)
        {
            _statusCallback?.Invoke(message);
            DebugLogger.Log($"[SemanticModelBuilder] {message}");
        }

        /// <summary>
        /// Writes text to a file using UTF-8 without BOM encoding and CRLF line endings
        /// </summary>
        private static void WriteTmdlFile(string path, string content)
        {
            // Ensure CRLF line endings (Power BI Desktop standard)
            content = content.Replace("\r\n", "\n").Replace("\n", "\r\n");
            File.WriteAllText(path, content, Utf8WithoutBom);
        }

        /// <summary>
        /// Builds the semantic model in the specified output folder
        /// </summary>
        public void Build(
            string semanticModelName,
            string outputFolder,
            string dataverseUrl,
            List<ExportTable> tables,
            List<ExportRelationship> relationships,
            Dictionary<string, Dictionary<string, AttributeDisplayInfo>> attributeDisplayInfo)
        {
            SetStatus("Starting semantic model build...");

            // Determine the PBIP folder path
            var pbipFolder = Path.Combine(outputFolder, "PBIP");
            var projectName = semanticModelName;
            var pbipFilePath = Path.Combine(pbipFolder, $"{projectName}.pbip");

            // Always start fresh to avoid corruption
            SetStatus("Preparing project folder...");
            if (Directory.Exists(pbipFolder))
            {
                Directory.Delete(pbipFolder, true);
            }
            
            SetStatus("Copying PBIP template...");
            CopyTemplate(pbipFolder, projectName);

            // Update project name and DataverseURL
            SetStatus("Updating project configuration...");
            UpdateProjectConfiguration(pbipFolder, projectName, dataverseUrl);

            // Build a dictionary of lookup columns needed for relationships per source table
            var relationshipColumnsPerTable = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var rel in relationships)
            {
                if (!relationshipColumnsPerTable.ContainsKey(rel.SourceTable))
                    relationshipColumnsPerTable[rel.SourceTable] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                relationshipColumnsPerTable[rel.SourceTable].Add(rel.SourceAttribute);
            }

            // Build tables
            SetStatus($"Building {tables.Count} tables...");
            var tablesFolder = Path.Combine(pbipFolder, $"{projectName}.SemanticModel", "definition", "tables");
            Directory.CreateDirectory(tablesFolder);

            foreach (var table in tables)
            {
                SetStatus($"Building table: {table.DisplayName}...");
                var requiredLookupColumns = relationshipColumnsPerTable.ContainsKey(table.LogicalName)
                    ? relationshipColumnsPerTable[table.LogicalName]
                    : new HashSet<string>();
                var tableTmdl = GenerateTableTmdl(table, attributeDisplayInfo, requiredLookupColumns);
                var tableFileName = SanitizeFileName(table.DisplayName ?? table.LogicalName) + ".tmdl";
                WriteTmdlFile(Path.Combine(tablesFolder, tableFileName), tableTmdl);
            }

            // Build relationships
            if (relationships.Any())
            {
                SetStatus($"Building {relationships.Count} relationships...");
                var relationshipsTmdl = GenerateRelationshipsTmdl(tables, relationships, attributeDisplayInfo);
                var definitionFolder = Path.Combine(pbipFolder, $"{projectName}.SemanticModel", "definition");
                WriteTmdlFile(Path.Combine(definitionFolder, "relationships.tmdl"), relationshipsTmdl);
            }

            // Update model.tmdl with table references
            SetStatus("Updating model configuration...");
            UpdateModelTmdl(pbipFolder, projectName, tables);

            SetStatus("Semantic model build complete!");
        }

        /// <summary>
        /// Analyzes what changes would be made without applying them
        /// </summary>
        public List<SemanticModelChange> AnalyzeChanges(
            string semanticModelName,
            string outputFolder,
            string dataverseUrl,
            List<ExportTable> tables,
            List<ExportRelationship> relationships,
            Dictionary<string, Dictionary<string, AttributeDisplayInfo>> attributeDisplayInfo)
        {
            SetStatus("Analyzing changes...");

            var pbipFolder = Path.Combine(outputFolder, "PBIP");
            var projectName = semanticModelName;
            bool pbipExists = Directory.Exists(pbipFolder);

            var changes = new List<SemanticModelChange>();

            if (!pbipExists)
            {
                // First time - create everything
                changes.Add(new SemanticModelChange
                {
                    ChangeType = ChangeType.New,
                    ObjectType = "Project",
                    ObjectName = projectName,
                    Description = "Create new Power BI project from template"
                });

                foreach (var table in tables)
                {
                    changes.Add(new SemanticModelChange
                    {
                        ChangeType = ChangeType.New,
                        ObjectType = "Table",
                        ObjectName = table.DisplayName ?? table.LogicalName,
                        Description = $"Create table with {table.Attributes?.Count ?? 0} columns"
                    });
                }

                foreach (var rel in relationships)
                {
                    changes.Add(new SemanticModelChange
                    {
                        ChangeType = ChangeType.New,
                        ObjectType = "Relationship",
                        ObjectName = $"{rel.SourceTable} → {rel.TargetTable}",
                        Description = $"via {rel.SourceAttribute}"
                    });
                }
            }
            else
            {
                // PBIP exists - analyze incremental changes with deep comparison
                var tablesFolder = Path.Combine(pbipFolder, $"{projectName}.SemanticModel", "definition", "tables");

                // Build relationship columns lookup
                var relationshipColumnsPerTable = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
                foreach (var rel in relationships)
                {
                    if (!relationshipColumnsPerTable.ContainsKey(rel.SourceTable))
                        relationshipColumnsPerTable[rel.SourceTable] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    relationshipColumnsPerTable[rel.SourceTable].Add(rel.SourceAttribute);
                }

                if (Directory.Exists(tablesFolder))
                {
                    var existingTables = Directory.GetFiles(tablesFolder, "*.tmdl")
                        .Select(f => Path.GetFileNameWithoutExtension(f))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    var metadataTables = tables.Select(t => SanitizeFileName(t.DisplayName ?? t.LogicalName))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    // Analyze each table for actual changes
                    foreach (var table in tables)
                    {
                        var fileName = SanitizeFileName(table.DisplayName ?? table.LogicalName);
                        var tmdlPath = Path.Combine(tablesFolder, fileName + ".tmdl");
                        
                        if (!existingTables.Contains(fileName))
                        {
                            // New table
                            changes.Add(new SemanticModelChange
                            {
                                ChangeType = ChangeType.New,
                                ObjectType = "Table",
                                ObjectName = table.DisplayName ?? table.LogicalName,
                                Description = $"Create new table with {table.Attributes?.Count ?? 0} columns"
                            });
                        }
                        else
                        {
                            // Table exists - deep comparison
                            var requiredLookupColumns = relationshipColumnsPerTable.ContainsKey(table.LogicalName)
                                ? relationshipColumnsPerTable[table.LogicalName]
                                : new HashSet<string>();

                            var tableChanges = AnalyzeTableChanges(tmdlPath, table, attributeDisplayInfo, requiredLookupColumns);

                            // Check for user measures to preserve
                            var userMeasures = ExtractUserMeasures(tmdlPath, table);
                            if (userMeasures.Count > 0)
                            {
                                changes.Add(new SemanticModelChange
                                {
                                    ChangeType = ChangeType.Preserve,
                                    ObjectType = "Measures",
                                    ObjectName = table.DisplayName ?? table.LogicalName,
                                    Description = $"Preserve {userMeasures.Count} user-created measure(s): {string.Join(", ", userMeasures.Take(3))}{(userMeasures.Count > 3 ? "..." : "")}"
                                });
                            }

                            // Add table changes if any
                            if (tableChanges.HasChanges)
                            {
                                var changeDetails = new List<string>();
                                if (tableChanges.QueryChanged) changeDetails.Add("query");
                                if (tableChanges.NewColumns.Count > 0) changeDetails.Add($"{tableChanges.NewColumns.Count} new column(s)");
                                if (tableChanges.ModifiedColumns.Count > 0) changeDetails.Add($"{tableChanges.ModifiedColumns.Count} modified column(s)");
                                if (tableChanges.RemovedColumns.Count > 0) changeDetails.Add($"{tableChanges.RemovedColumns.Count} removed column(s)");

                                changes.Add(new SemanticModelChange
                                {
                                    ChangeType = ChangeType.Update,
                                    ObjectType = "Table",
                                    ObjectName = table.DisplayName ?? table.LogicalName,
                                    Description = $"Update: {string.Join(", ", changeDetails)}"
                                });

                                // Add detailed changes
                                foreach (var col in tableChanges.NewColumns)
                                {
                                    changes.Add(new SemanticModelChange
                                    {
                                        ChangeType = ChangeType.New,
                                        ObjectType = "Column",
                                        ObjectName = $"{table.DisplayName}.{col}",
                                        Description = "New column"
                                    });
                                }

                                foreach (var kvp in tableChanges.ModifiedColumns)
                                {
                                    changes.Add(new SemanticModelChange
                                    {
                                        ChangeType = ChangeType.Update,
                                        ObjectType = "Column",
                                        ObjectName = $"{table.DisplayName}.{kvp.Key}",
                                        Description = $"Changed: {kvp.Value}"
                                    });
                                }
                            }
                            else
                            {
                                // No changes - preserve as-is
                                changes.Add(new SemanticModelChange
                                {
                                    ChangeType = ChangeType.Preserve,
                                    ObjectType = "Table",
                                    ObjectName = table.DisplayName ?? table.LogicalName,
                                    Description = "No changes detected"
                                });
                            }
                        }
                    }

                    // Warn about orphaned tables
                    var orphanedTables = existingTables.Except(metadataTables, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var orphan in orphanedTables)
                    {
                        changes.Add(new SemanticModelChange
                        {
                            ChangeType = ChangeType.Warning,
                            ObjectType = "Table",
                            ObjectName = orphan,
                            Description = "Exists in PBIP but not in Dataverse metadata (will be kept as-is)"
                        });
                    }
                }

                // Check for relationship changes
                var relationshipChanges = AnalyzeRelationshipChanges(pbipFolder, projectName, relationships, tables, attributeDisplayInfo);
                if (relationshipChanges.HasChanges)
                {
                    var relDetails = new List<string>();
                    if (relationshipChanges.NewRelationships.Count > 0) relDetails.Add($"{relationshipChanges.NewRelationships.Count} new");
                    if (relationshipChanges.ModifiedRelationships.Count > 0) relDetails.Add($"{relationshipChanges.ModifiedRelationships.Count} modified");
                    if (relationshipChanges.RemovedRelationships.Count > 0) relDetails.Add($"{relationshipChanges.RemovedRelationships.Count} removed");

                    changes.Add(new SemanticModelChange
                    {
                        ChangeType = ChangeType.Update,
                        ObjectType = "Relationships",
                        ObjectName = "All",
                        Description = $"Update: {string.Join(", ", relDetails)}"
                    });
                }
                else
                {
                    changes.Add(new SemanticModelChange
                    {
                        ChangeType = ChangeType.Preserve,
                        ObjectType = "Relationships",
                        ObjectName = "All",
                        Description = "No changes detected"
                    });
                }

                // Check DataverseURL changes
                var currentUrl = ExtractDataverseUrl(pbipFolder, projectName);
                if (!string.Equals(currentUrl, dataverseUrl, StringComparison.OrdinalIgnoreCase))
                {
                    changes.Add(new SemanticModelChange
                    {
                        ChangeType = ChangeType.Update,
                        ObjectType = "DataverseURL",
                        ObjectName = "Expression",
                        Description = $"Update: {currentUrl} → {dataverseUrl}"
                    });
                }
                else
                {
                    changes.Add(new SemanticModelChange
                    {
                        ChangeType = ChangeType.Preserve,
                        ObjectType = "DataverseURL",
                        ObjectName = "Expression",
                        Description = "No changes detected"
                    });
                }
            }

            // Summarize
            var summary = changes.GroupBy(c => c.ChangeType).ToDictionary(g => g.Key, g => g.Count());
            var totalChanges = summary.Where(kvp => kvp.Key == ChangeType.New || kvp.Key == ChangeType.Update).Sum(kvp => kvp.Value);
            
            if (totalChanges == 0)
            {
                SetStatus("No changes detected - model is up to date");
            }
            else
            {
                SetStatus($"Detected {totalChanges} change(s) requiring update");
            }

            return changes;
        }

        /// <summary>
        /// Deeply analyzes a table for actual changes
        /// </summary>
        private TableChangeAnalysis AnalyzeTableChanges(
            string tmdlPath,
            ExportTable table,
            Dictionary<string, Dictionary<string, AttributeDisplayInfo>> attributeDisplayInfo,
            HashSet<string> requiredLookupColumns)
        {
            var analysis = new TableChangeAnalysis();

            if (!File.Exists(tmdlPath))
            {
                analysis.QueryChanged = true; // File doesn't exist, treat as all changes
                return analysis;
            }

            try
            {
                var existingContent = File.ReadAllText(tmdlPath);

                // Parse existing columns
                var existingColumns = ParseExistingColumns(existingContent);

                // Generate what the new columns should be (pass existing to preserve lookup column types)
                var newColumns = GenerateExpectedColumns(table, attributeDisplayInfo, requiredLookupColumns, existingColumns);

                // DEBUG: Log column comparison for Allocation
                if (table.LogicalName == "cai_allocation")
                {
                    DebugLogger.Log($"Allocation column comparison:");
                    DebugLogger.Log($"  Existing ({existingColumns.Count}): {string.Join(", ", existingColumns.Keys.Take(5))}...");
                    DebugLogger.Log($"  Expected ({newColumns.Count}): {string.Join(", ", newColumns.Keys.Take(5))}...");
                }

                // Compare columns
                foreach (var newCol in newColumns)
                {
                    if (!existingColumns.ContainsKey(newCol.Key))
                    {
                        analysis.NewColumns.Add(newCol.Key);
                    }
                    else
                    {
                        var existing = existingColumns[newCol.Key];
                        var diffs = CompareColumnDefinitions(existing, newCol.Value);
                        if (diffs.Count > 0)
                        {
                            analysis.ModifiedColumns[newCol.Key] = string.Join(", ", diffs);
                        }
                    }
                }

                // Check for removed columns (only Dataverse columns, not user-added)
                foreach (var existingCol in existingColumns)
                {
                    if (existingCol.Value.LogicalName != null && !newColumns.ContainsKey(existingCol.Key))
                    {
                        analysis.RemovedColumns.Add(existingCol.Key);
                        DebugLogger.Log($"Removed column detected in {table.DisplayName ?? table.LogicalName}: {existingCol.Key} (logical: {existingCol.Value.LogicalName})");
                    }
                }

                // Compare M query
                var existingQuery = ExtractMQuery(existingContent);
                var requiredLookupCols = requiredLookupColumns ?? new HashSet<string>();
                var newQuery = GenerateMQuery(table, requiredLookupCols);
                
                // Only flag as changed if queries actually differ
                if (!string.IsNullOrEmpty(existingQuery) && !string.IsNullOrEmpty(newQuery))
                {
                    if (!CompareQueries(existingQuery, newQuery))
                    {
                        analysis.QueryChanged = true;
                        DebugLogger.Log($"Query mismatch for {table.DisplayName ?? table.LogicalName}");
                        DebugLogger.Log($"Existing: {existingQuery.Substring(0, Math.Min(200, existingQuery.Length))}");
                        DebugLogger.Log($"Expected: {newQuery.Substring(0, Math.Min(200, newQuery.Length))}");
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"Warning: Could not analyze table changes for {tmdlPath}: {ex.Message}");
                analysis.QueryChanged = true; // Assume changes if we can't parse
            }

            return analysis;
        }

        /// <summary>
        /// Parses existing column definitions from TMDL
        /// </summary>
        private Dictionary<string, ColumnDefinition> ParseExistingColumns(string tmdlContent)
        {
            var columns = new Dictionary<string, ColumnDefinition>(StringComparer.OrdinalIgnoreCase);
            
            // Match column blocks with optional /// comment
            // Pattern captures: comment, display name (quoted or unquoted), and properties block
            var columnPattern = @"(?:///\s*([^\r\n]+)\r?\n)?\s*column\s+(?:'([^']+)'|""([^""]+)""|([^\r\n]+))\r?\n((?:\t[^\r\n]+\r?\n)+)";
            var matches = Regex.Matches(tmdlContent, columnPattern, RegexOptions.Multiline);

            foreach (Match match in matches)
            {
                var logicalName = match.Groups[1].Success ? match.Groups[1].Value.Trim() : null;
                var displayName = match.Groups[2].Success ? match.Groups[2].Value :
                                 match.Groups[3].Success ? match.Groups[3].Value :
                                 match.Groups[4].Value.Trim();
                var properties = match.Groups[5].Value;

                // Parse properties - dataType is the word after "dataType:"
                var dataTypeMatch = Regex.Match(properties, @"\bdataType:\s*([^\r\n]+)");
                var sourceColumnMatch = Regex.Match(properties, @"\bsourceColumn:\s*([^\r\n]+)");
                var formatStringMatch = Regex.Match(properties, @"\bformatString:\s*([^\r\n]+)");

                columns[displayName] = new ColumnDefinition
                {
                    DisplayName = displayName,
                    LogicalName = logicalName,
                    DataType = dataTypeMatch.Success ? dataTypeMatch.Groups[1].Value.Trim() : null,
                    SourceColumn = sourceColumnMatch.Success ? sourceColumnMatch.Groups[1].Value.Trim() : null,
                    FormatString = formatStringMatch.Success ? formatStringMatch.Groups[1].Value.Trim() : null
                };
            }

            return columns;
        }

        /// <summary>
        /// Generates expected column definitions for comparison
        /// </summary>
        private Dictionary<string, ColumnDefinition> GenerateExpectedColumns(
            ExportTable table,
            Dictionary<string, Dictionary<string, AttributeDisplayInfo>> attributeDisplayInfo,
            HashSet<string> requiredLookupColumns,
            Dictionary<string, ColumnDefinition> existingColumns = null)
        {
            var columns = new Dictionary<string, ColumnDefinition>(StringComparer.OrdinalIgnoreCase);

            var attrInfo = attributeDisplayInfo.ContainsKey(table.LogicalName)
                ? attributeDisplayInfo[table.LogicalName]
                : new Dictionary<string, AttributeDisplayInfo>();

            var processedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Include primary key (always uses logical name as display name when hidden)
            var primaryKey = table.PrimaryIdAttribute ?? table.LogicalName + "id";
            if (table.Attributes?.Any(a => a.LogicalName == primaryKey) == true)
            {
                var pkAttr = table.Attributes.First(a => a.LogicalName == primaryKey);
                
                var (dataType, formatString, _, _) = MapDataType(pkAttr.AttributeType);
                columns[pkAttr.LogicalName] = new ColumnDefinition
                {
                    DisplayName = pkAttr.LogicalName, // Primary key uses logical name
                    LogicalName = pkAttr.LogicalName,
                    DataType = dataType,
                    SourceColumn = pkAttr.LogicalName,
                    FormatString = formatString
                };
                processedColumns.Add(primaryKey);
            }

            // Process each attribute matching the actual generation logic
            if (table.Attributes != null)
            {
                foreach (var attr in table.Attributes)
                {
                    if (processedColumns.Contains(attr.LogicalName)) continue;

                    var attrDisplayInfo = attrInfo.ContainsKey(attr.LogicalName) ? attrInfo[attr.LogicalName] : null;
                    var attrType = attr.AttributeType ?? attrDisplayInfo?.AttributeType ?? "";
                    // CRITICAL: Match the same priority as TMDL generation code
                    var attrDisplayName = attr.DisplayName ?? attrDisplayInfo?.DisplayName ?? attr.SchemaName ?? attr.LogicalName;
                    var targets = attr.Targets ?? attrDisplayInfo?.Targets;

                    // Skip statecode
                    if (attr.LogicalName.Equals("statecode", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var isLookup = attrType.Equals("Lookup", StringComparison.OrdinalIgnoreCase) ||
                                   attrType.Equals("Owner", StringComparison.OrdinalIgnoreCase) ||
                                   attrType.Equals("Customer", StringComparison.OrdinalIgnoreCase);
                    var isChoice = attrType.Equals("Picklist", StringComparison.OrdinalIgnoreCase) ||
                                   attrType.Equals("State", StringComparison.OrdinalIgnoreCase) ||
                                   attrType.Equals("Status", StringComparison.OrdinalIgnoreCase);
                    var isBoolean = attrType.Equals("Boolean", StringComparison.OrdinalIgnoreCase);

                    if (isLookup)
                    {
                        // Hidden ID column
                        var (dataType, formatString, _, _) = MapDataType("lookup");
                        columns[attr.LogicalName] = new ColumnDefinition
                        {
                            DisplayName = attr.LogicalName,
                            LogicalName = attr.LogicalName,
                            DataType = dataType,
                            SourceColumn = attr.LogicalName,
                            FormatString = formatString
                        };

                        // Visible name column
                        var nameColumn = attr.LogicalName + "name";
                        columns[attrDisplayName] = new ColumnDefinition
                        {
                            DisplayName = attrDisplayName,
                            LogicalName = nameColumn,
                            DataType = "string",
                            SourceColumn = nameColumn,
                            FormatString = null
                        };
                    }
                    else if (isChoice || isBoolean)
                    {
                        // Choice/Boolean: use fieldname with "name" appended
                        var nameColumn = attr.LogicalName + "name";
                        columns[attrDisplayName] = new ColumnDefinition
                        {
                            DisplayName = attrDisplayName,
                            LogicalName = nameColumn,
                            DataType = "string",
                            SourceColumn = nameColumn,
                            FormatString = null
                        };
                    }
                    else
                    {
                        // Regular column
                        var (dataType, formatString, _, _) = MapDataType(attr.AttributeType);
                        columns[attrDisplayName] = new ColumnDefinition
                        {
                            DisplayName = attrDisplayName,
                            LogicalName = attr.LogicalName,
                            DataType = dataType,
                            SourceColumn = attr.LogicalName,
                            FormatString = formatString
                        };
                    }
                }
            }

            // Add any missing lookup columns from requiredLookupColumns that weren't in table.Attributes
            // These are needed for relationships but might not be selected as attributes (e.g., owninguser)
            // IMPORTANT: If column exists in TMDL, preserve its dataType/formatString to avoid false changes
            foreach (var lookupCol in requiredLookupColumns)
            {
                if (!processedColumns.Contains(lookupCol))
                {
                    // Check if this column exists in the TMDL
                    if (existingColumns != null && existingColumns.TryGetValue(lookupCol, out var existingCol))
                    {
                        // Use existing column definition to preserve dataType, formatString, etc.
                        columns[lookupCol] = existingCol;
                    }
                    else
                    {
                        // New column - use default lookup type
                        columns[lookupCol] = new ColumnDefinition
                        {
                            DisplayName = lookupCol,
                            LogicalName = lookupCol,
                            DataType = "int64",
                            SourceColumn = lookupCol,
                            FormatString = "0"
                        };
                    }
                    processedColumns.Add(lookupCol);
                }
            }

            return columns;
        }

        /// <summary>
        /// Compares two column definitions
        /// </summary>
        private List<string> CompareColumnDefinitions(ColumnDefinition existing, ColumnDefinition expected)
        {
            var diffs = new List<string>();

            if (existing.DataType != expected.DataType)
                diffs.Add($"dataType: {existing.DataType} → {expected.DataType}");

            if (existing.DisplayName != expected.DisplayName)
                diffs.Add($"displayName: {existing.DisplayName} → {expected.DisplayName}");

            if (existing.FormatString != expected.FormatString && 
                !(string.IsNullOrEmpty(existing.FormatString) && string.IsNullOrEmpty(expected.FormatString)))
                diffs.Add($"formatString changed");

            return diffs;
        }

        /// <summary>
        /// Extracts M query from partition section
        /// </summary>
        private string ExtractMQuery(string tmdlContent)
        {
            var partitionMatch = Regex.Match(tmdlContent, @"partition\s+.*?\s*=\s*\r?\n\s*m\r?\n(.*?)(?=\r?\n\s*annotation)", RegexOptions.Singleline);
            if (partitionMatch.Success)
            {
                return NormalizeQuery(partitionMatch.Groups[1].Value);
            }
            return string.Empty;
        }

        /// <summary>
        /// Generates expected M query for comparison - must exactly match actual BuildIncrementalTable logic
        /// </summary>
        private string GenerateMQuery(ExportTable table, HashSet<string> requiredLookupColumns)
        {
            var schemaName = table.SchemaName ?? table.LogicalName;
            var sqlFields = new List<string>();
            var processedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Add primary key first
            var primaryKey = table.PrimaryIdAttribute ?? table.LogicalName + "id";
            sqlFields.Add($"Base.{primaryKey}");
            processedColumns.Add(primaryKey);

            // Add required lookup columns that aren't the primary key
            foreach (var lookupCol in requiredLookupColumns)
            {
                if (!processedColumns.Contains(lookupCol))
                {
                    sqlFields.Add($"Base.{lookupCol}");
                    processedColumns.Add(lookupCol);
                }
            }

            // Process attributes matching the exact logic in BuildIncrementalTable
            if (table.Attributes != null)
            {
                foreach (var attr in table.Attributes)
                {
                    if (processedColumns.Contains(attr.LogicalName)) continue;

                    var attrType = attr.AttributeType ?? "";

                    // Skip statecode
                    if (attr.LogicalName.Equals("statecode", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var isLookup = attrType.Equals("Lookup", StringComparison.OrdinalIgnoreCase) ||
                                   attrType.Equals("Owner", StringComparison.OrdinalIgnoreCase) ||
                                   attrType.Equals("Customer", StringComparison.OrdinalIgnoreCase);
                    var isChoice = attrType.Equals("Picklist", StringComparison.OrdinalIgnoreCase) ||
                                   attrType.Equals("State", StringComparison.OrdinalIgnoreCase) ||
                                   attrType.Equals("Status", StringComparison.OrdinalIgnoreCase);
                    var isBoolean = attrType.Equals("Boolean", StringComparison.OrdinalIgnoreCase);

                    if (isLookup)
                    {
                        // Add both ID and name columns
                        sqlFields.Add($"Base.{attr.LogicalName}");
                        sqlFields.Add($"Base.{attr.LogicalName}name");
                    }
                    else if (isChoice || isBoolean)
                    {
                        // Add name column only
                        sqlFields.Add($"Base.{attr.LogicalName}name");
                    }
                    else
                    {
                        // Regular column
                        sqlFields.Add($"Base.{attr.LogicalName}");
                    }
                }
            }

            var selectList = string.Join(", ", sqlFields);
            var whereClause = table.HasStateCode ? " WHERE statecode=0" : "";
            
            return NormalizeQuery($"SELECT {selectList} FROM {schemaName} AS Base{whereClause}");
        }

        /// <summary>
        /// Normalizes queries for comparison (removes all whitespace and formatting differences)
        /// </summary>
        private string NormalizeQuery(string query)
        {
            if (string.IsNullOrEmpty(query)) return string.Empty;
            
            // Remove ALL whitespace for comparison - this handles different formatting styles
            // (single line vs multi-line, different indentation, etc.)
            return Regex.Replace(query.Trim().ToUpperInvariant(), @"\s+", "");
        }

        /// <summary>
        /// Compares two queries
        /// </summary>
        private bool CompareQueries(string existing, string expected)
        {
            return string.Equals(existing, expected, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Analyzes relationship changes
        /// </summary>
        private RelationshipChangeAnalysis AnalyzeRelationshipChanges(
            string pbipFolder,
            string projectName,
            List<ExportRelationship> newRelationships,
            List<ExportTable> tables,
            Dictionary<string, Dictionary<string, AttributeDisplayInfo>> attributeDisplayInfo)
        {
            var analysis = new RelationshipChangeAnalysis();
            var relationshipsPath = Path.Combine(pbipFolder, $"{projectName}.SemanticModel", "definition", "relationships.tmdl");

            if (!File.Exists(relationshipsPath))
            {
                // File doesn't exist - all relationships would be new
                var tableDisplayNames = tables.ToDictionary(t => t.LogicalName, t => t.DisplayName ?? t.LogicalName, StringComparer.OrdinalIgnoreCase);
                var tablePrimaryKeys = tables.ToDictionary(t => t.LogicalName, t => t.PrimaryIdAttribute ?? $"{t.LogicalName}id", StringComparer.OrdinalIgnoreCase);
                
                analysis.NewRelationships.AddRange(newRelationships.Select(r =>
                {
                    var sourceTable = tableDisplayNames.ContainsKey(r.SourceTable) ? tableDisplayNames[r.SourceTable] : r.SourceTable;
                    var targetTable = tableDisplayNames.ContainsKey(r.TargetTable) ? tableDisplayNames[r.TargetTable] : r.TargetTable;
                    var targetAttr = tablePrimaryKeys.ContainsKey(r.TargetTable) ? tablePrimaryKeys[r.TargetTable] : $"{r.TargetTable}id";
                    return $"{sourceTable}.{r.SourceAttribute} → {targetTable}.{targetAttr}";
                }));
                return analysis;
            }

            try
            {
                var existingContent = File.ReadAllText(relationshipsPath);
                var existingRels = ParseExistingRelationships(existingContent);
                var expectedRels = GenerateExpectedRelationships(newRelationships, tables, attributeDisplayInfo);

                // DEBUG: Log what we're comparing
                DebugLogger.Log($"Relationship comparison:");
                DebugLogger.Log($"  Existing ({existingRels.Count}): {string.Join(", ", existingRels.Take(3))}...");
                DebugLogger.Log($"  Expected ({expectedRels.Count}): {string.Join(", ", expectedRels.Take(3))}...");

                // Compare
                foreach (var expected in expectedRels)
                {
                    if (!existingRels.Contains(expected))
                    {
                        analysis.NewRelationships.Add(expected);
                        DebugLogger.Log($"  NEW: {expected}");
                    }
                }

                foreach (var existing in existingRels)
                {
                    if (!expectedRels.Contains(existing))
                    {
                        analysis.RemovedRelationships.Add(existing);
                        DebugLogger.Log($"  REMOVED: {existing}");
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"Warning: Could not analyze relationship changes: {ex.Message}");
                // Fallback - treat all as new
                var tableDisplayNames = tables.ToDictionary(t => t.LogicalName, t => t.DisplayName ?? t.LogicalName, StringComparer.OrdinalIgnoreCase);
                var tablePrimaryKeys = tables.ToDictionary(t => t.LogicalName, t => t.PrimaryIdAttribute ?? $"{t.LogicalName}id", StringComparer.OrdinalIgnoreCase);
                
                analysis.NewRelationships.AddRange(newRelationships.Select(r =>
                {
                    var sourceTable = tableDisplayNames.ContainsKey(r.SourceTable) ? tableDisplayNames[r.SourceTable] : r.SourceTable;
                    var targetTable = tableDisplayNames.ContainsKey(r.TargetTable) ? tableDisplayNames[r.TargetTable] : r.TargetTable;
                    var targetAttr = tablePrimaryKeys.ContainsKey(r.TargetTable) ? tablePrimaryKeys[r.TargetTable] : $"{r.TargetTable}id";
                    return $"{sourceTable}.{r.SourceAttribute} → {targetTable}.{targetAttr}";
                }));
            }

            return analysis;
        }

        /// <summary>
        /// Parses existing relationships from TMDL
        /// </summary>
        private HashSet<string> ParseExistingRelationships(string content)
        {
            var rels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            DebugLogger.Log($"Parsing relationships from TMDL content ({content.Length} chars)");
            
            // Match relationship blocks that may have properties on separate lines
            // Pattern: relationship <guid> ... everything until next relationship or end of string
            // Use (?=\r?\nrelationship\s) to match until newline+relationship (not just any whitespace after "relationship")
            var relPattern = @"relationship\s+[a-f0-9\-]+[\s\S]*?(?=\r?\nrelationship\s|\Z)";
            var matches = Regex.Matches(content, relPattern, RegexOptions.Multiline);
            
            DebugLogger.Log($"Found {matches.Count} relationship blocks");

            foreach (Match match in matches)
            {
                var relBlock = match.Value;
                var escapedBlock = relBlock.Replace("\r", "").Replace("\n", " | ").Replace("\t", "[TAB]");
                DebugLogger.Log($"Block content: {escapedBlock}");
                
                // Extract fromColumn and toColumn from the block
                // TMDL format: fromColumn: Table.Column (table and column separated by dot, may have quotes around names with spaces)
                var fromMatch = Regex.Match(relBlock, @"fromColumn:\s*(?:'([^']+)'|([^\s.]+))\.(?:'([^']+)'|([^\s\r\n.]+))", RegexOptions.Multiline);
                var toMatch = Regex.Match(relBlock, @"toColumn:\s*(?:'([^']+)'|([^\s.]+))\.(?:'([^']+)'|([^\s\r\n.]+))", RegexOptions.Multiline);
                
                DebugLogger.Log($"  fromMatch.Success={fromMatch.Success}, toMatch.Success={toMatch.Success}");
                if (fromMatch.Success) 
                {
                    DebugLogger.Log($"  fromColumn matched: {fromMatch.Value}");
                    DebugLogger.Log($"    Groups: [1]='{fromMatch.Groups[1].Value}' [2]='{fromMatch.Groups[2].Value}' [3]='{fromMatch.Groups[3].Value}' [4]='{fromMatch.Groups[4].Value}'");
                }
                else
                {
                    DebugLogger.Log($"  fromColumn FAILED to match in block");
                }
                
                if (toMatch.Success) 
                {
                    DebugLogger.Log($"  toColumn matched: {toMatch.Value}");
                    DebugLogger.Log($"    Groups: [1]='{toMatch.Groups[1].Value}' [2]='{toMatch.Groups[2].Value}' [3]='{toMatch.Groups[3].Value}' [4]='{toMatch.Groups[4].Value}'");
                }
                else
                {
                    DebugLogger.Log($"  toColumn FAILED to match in block");
                }
                
                if (fromMatch.Success && toMatch.Success)
                {
                    var fromTable = fromMatch.Groups[1].Success ? fromMatch.Groups[1].Value : fromMatch.Groups[2].Value;
                    var fromCol = fromMatch.Groups[3].Success ? fromMatch.Groups[3].Value : fromMatch.Groups[4].Value;
                    var toTable = toMatch.Groups[1].Success ? toMatch.Groups[1].Value : toMatch.Groups[2].Value;
                    var toCol = toMatch.Groups[3].Success ? toMatch.Groups[3].Value : toMatch.Groups[4].Value;

                    var relString = $"{fromTable}.{fromCol}→{toTable}.{toCol}";
                    rels.Add(relString);
                    DebugLogger.Log($"  Parsed TMDL: {relString}");
                }
            }

            return rels;
        }

        /// <summary>
        /// Generates expected relationships for comparison
        /// </summary>
        private HashSet<string> GenerateExpectedRelationships(
            List<ExportRelationship> relationships,
            List<ExportTable> tables,
            Dictionary<string, Dictionary<string, AttributeDisplayInfo>> attributeDisplayInfo)
        {
            var rels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var tableDisplayNames = tables.ToDictionary(t => t.LogicalName, t => t.DisplayName ?? t.LogicalName, StringComparer.OrdinalIgnoreCase);
            var tablePrimaryKeys = tables.ToDictionary(t => t.LogicalName, t => t.PrimaryIdAttribute ?? $"{t.LogicalName}id", StringComparer.OrdinalIgnoreCase);

            foreach (var rel in relationships)
            {
                var sourceTableDisplay = tableDisplayNames.ContainsKey(rel.SourceTable) ? tableDisplayNames[rel.SourceTable] : rel.SourceTable;
                var targetTableDisplay = tableDisplayNames.ContainsKey(rel.TargetTable) ? tableDisplayNames[rel.TargetTable] : rel.TargetTable;

                // CRITICAL: Use logical names, not display names, as TMDL relationships use logical names
                var sourceColLogical = rel.SourceAttribute; // This is the lookup ID field

                // Target is the primary key of the target table
                var targetColLogical = tablePrimaryKeys.ContainsKey(rel.TargetTable) ? tablePrimaryKeys[rel.TargetTable] : $"{rel.TargetTable}id";

                var relString = $"{sourceTableDisplay}.{sourceColLogical}→{targetTableDisplay}.{targetColLogical}";
                rels.Add(relString);
                DebugLogger.Log($"  Generated: {relString} (from {rel.SourceTable}.{rel.SourceAttribute} to {rel.TargetTable})");
            }

            return rels;
        }

        /// <summary>
        /// Extracts current DataverseURL from expressions.tmdl
        /// </summary>
        private string ExtractDataverseUrl(string pbipFolder, string projectName)
        {
            var expressionsPath = Path.Combine(pbipFolder, $"{projectName}.SemanticModel", "definition", "expressions.tmdl");
            if (!File.Exists(expressionsPath))
                return string.Empty;

            try
            {
                var content = File.ReadAllText(expressionsPath);
                var urlMatch = Regex.Match(content, @"DataverseURL\s*=\s*""([^""]+)""");
                if (urlMatch.Success)
                    return urlMatch.Groups[1].Value;
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"Warning: Could not extract DataverseURL: {ex.Message}");
            }

            return string.Empty;
        }

        /// <summary>
        /// Column definition for comparison
        /// </summary>
        private class ColumnDefinition
        {
            public string? DisplayName { get; set; }
            public string? LogicalName { get; set; }
            public string? DataType { get; set; }
            public string? SourceColumn { get; set; }
            public string? FormatString { get; set; }
        }

        /// <summary>
        /// Table change analysis result
        /// </summary>
        private class TableChangeAnalysis
        {
            public bool QueryChanged { get; set; }
            public List<string> NewColumns { get; } = new List<string>();
            public Dictionary<string, string> ModifiedColumns { get; } = new Dictionary<string, string>();
            public List<string> RemovedColumns { get; } = new List<string>();

            public bool HasChanges => QueryChanged || NewColumns.Count > 0 || ModifiedColumns.Count > 0 || RemovedColumns.Count > 0;
        }

        /// <summary>
        /// Relationship change analysis result
        /// </summary>
        private class RelationshipChangeAnalysis
        {
            public List<string> NewRelationships { get; } = new List<string>();
            public List<string> ModifiedRelationships { get; } = new List<string>();
            public List<string> RemovedRelationships { get; } = new List<string>();

            public bool HasChanges => NewRelationships.Count > 0 || ModifiedRelationships.Count > 0 || RemovedRelationships.Count > 0;
        }

        /// <summary>
        /// Applies changes to the semantic model
        /// </summary>
        public bool ApplyChanges(
            string semanticModelName,
            string outputFolder,
            string dataverseUrl,
            List<ExportTable> tables,
            List<ExportRelationship> relationships,
            Dictionary<string, Dictionary<string, AttributeDisplayInfo>> attributeDisplayInfo,
            bool createBackup)
        {
            try
            {
                var pbipFolder = Path.Combine(outputFolder, "PBIP");
                bool pbipExists = Directory.Exists(pbipFolder);

                // Create backup if requested
                if (createBackup && pbipExists)
                {
                    CreateBackup(pbipFolder, outputFolder);
                }

                // Apply changes
                if (pbipExists)
                {
                    BuildIncremental(semanticModelName, outputFolder, dataverseUrl, tables, relationships, attributeDisplayInfo);
                }
                else
                {
                    Build(semanticModelName, outputFolder, dataverseUrl, tables, relationships, attributeDisplayInfo);
                }

                return true;
            }
            catch (Exception ex)
            {
                SetStatus($"Error applying changes: {ex.Message}");
                DebugLogger.Log($"Error in ApplyChanges: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Creates a timestamped backup of the PBIP folder
        /// </summary>
        private void CreateBackup(string pbipFolder, string outputFolder)
        {
            try
            {
                SetStatus("Creating backup...");
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFolder = Path.Combine(outputFolder, $"PBIP_Backup_{timestamp}");
                
                // Simple directory copy for backup (no template replacement needed)
                CopyDirectorySimple(pbipFolder, backupFolder);
                SetStatus($"Backup created: {Path.GetFileName(backupFolder)}");
            }
            catch (Exception ex)
            {
                SetStatus($"Warning: Backup failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Simple directory copy without template replacement
        /// </summary>
        private void CopyDirectorySimple(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string targetFile = Path.Combine(targetDir, fileName);
                File.Copy(file, targetFile, true);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(subDir);
                string targetSubDir = Path.Combine(targetDir, dirName);
                CopyDirectorySimple(subDir, targetSubDir);
            }
        }

        /// <summary>
        /// Extracts user-created measures from existing TMDL (measures not from Dataverse)
        /// </summary>
        private List<string> ExtractUserMeasures(string tmdlPath, ExportTable table)
        {
            var userMeasures = new List<string>();

            if (!File.Exists(tmdlPath))
                return userMeasures;

            try
            {
                var content = File.ReadAllText(tmdlPath);
                var measurePattern = @"^\s*measure\s+([^\s]+)";
                var matches = Regex.Matches(content, measurePattern, RegexOptions.Multiline);

                foreach (Match match in matches)
                {
                    var measureName = match.Groups[1].Value.Trim('\'', '"', '[', ']');
                    // All measures are user-created (we don't generate measures from Dataverse)
                    userMeasures.Add(measureName);
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"Warning: Could not parse measures from {tmdlPath}: {ex.Message}");
            }

            return userMeasures;
        }

        /// <summary>
        /// Performs incremental update to existing PBIP, preserving user changes
        /// </summary>
        private void BuildIncremental(
            string semanticModelName,
            string outputFolder,
            string dataverseUrl,
            List<ExportTable> tables,
            List<ExportRelationship> relationships,
            Dictionary<string, Dictionary<string, AttributeDisplayInfo>> attributeDisplayInfo)
        {
            SetStatus("Performing incremental update...");

            var pbipFolder = Path.Combine(outputFolder, "PBIP");
            var projectName = semanticModelName;

            // Update project configuration (DataverseURL)
            SetStatus("Updating Dataverse URL...");
            UpdateProjectConfiguration(pbipFolder, projectName, dataverseUrl);

            // Build relationship columns lookup
            var relationshipColumnsPerTable = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var rel in relationships)
            {
                if (!relationshipColumnsPerTable.ContainsKey(rel.SourceTable))
                    relationshipColumnsPerTable[rel.SourceTable] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                relationshipColumnsPerTable[rel.SourceTable].Add(rel.SourceAttribute);
            }

            // Update tables incrementally
            SetStatus($"Updating {tables.Count} table(s)...");
            var tablesFolder = Path.Combine(pbipFolder, $"{projectName}.SemanticModel", "definition", "tables");
            Directory.CreateDirectory(tablesFolder);

            foreach (var table in tables)
            {
                SetStatus($"Updating table: {table.DisplayName}...");
                var tableFileName = SanitizeFileName(table.DisplayName ?? table.LogicalName) + ".tmdl";
                var tablePath = Path.Combine(tablesFolder, tableFileName);

                var requiredLookupColumns = relationshipColumnsPerTable.ContainsKey(table.LogicalName)
                    ? relationshipColumnsPerTable[table.LogicalName]
                    : new HashSet<string>();

                // Extract user measures if table exists
                string? userMeasuresSection = null;
                if (File.Exists(tablePath))
                {
                    userMeasuresSection = ExtractUserMeasuresSection(tablePath);
                }

                // Generate new table TMDL
                var tableTmdl = GenerateTableTmdl(table, attributeDisplayInfo, requiredLookupColumns);

                // Append user measures if any
                if (!string.IsNullOrEmpty(userMeasuresSection))
                {
                    tableTmdl = InsertUserMeasures(tableTmdl, userMeasuresSection);
                }

                WriteTmdlFile(tablePath, tableTmdl);
            }

            // Update relationships
            SetStatus("Updating relationships...");
            var relationshipsTmdl = GenerateRelationshipsTmdl(tables, relationships, attributeDisplayInfo);
            var relationshipsPath = Path.Combine(pbipFolder, $"{projectName}.SemanticModel", "definition", "relationships.tmdl");
            WriteTmdlFile(relationshipsPath, relationshipsTmdl);

            // Update model.tmdl
            SetStatus("Updating model metadata...");
            UpdateModelTmdl(pbipFolder, projectName, tables);

            SetStatus("Incremental update complete!");
        }

        /// <summary>
        /// Extracts the measures section from existing TMDL
        /// </summary>
        private string? ExtractUserMeasuresSection(string tmdlPath)
        {
            try
            {
                var content = File.ReadAllText(tmdlPath);
                
                // Find all measure blocks
                var measurePattern = @"(^\s*(?:///[^\r\n]*\r?\n)*\s*measure\s+[^\r\n]+\r?\n(?:.*?\r?\n)*?(?=^\s*(?:measure|column|partition|annotation)\s|\z))";
                var matches = Regex.Matches(content, measurePattern, RegexOptions.Multiline);

                if (matches.Count == 0)
                    return null;

                var sb = new StringBuilder();
                foreach (Match match in matches)
                {
                    sb.Append(match.Value);
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"Warning: Could not extract measures from {tmdlPath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Inserts user measures into generated TMDL (after columns, before partition)
        /// </summary>
        private string InsertUserMeasures(string tableTmdl, string measuresSection)
        {
            // Find the partition section and insert measures before it
            var partitionIndex = tableTmdl.IndexOf("\tpartition");
            if (partitionIndex > 0)
            {
                return tableTmdl.Insert(partitionIndex, measuresSection);
            }

            // If no partition found, append before annotations
            var annotationIndex = tableTmdl.IndexOf("\tannotation");
            if (annotationIndex > 0)
            {
                return tableTmdl.Insert(annotationIndex, measuresSection);
            }

            // Fallback: append at end
            return tableTmdl + measuresSection;
        }

        /// <summary>
        /// Updates the model.tmdl file with table references
        /// </summary>
        private void UpdateModelTmdl(string pbipFolder, string projectName, List<ExportTable> tables)
        {
            var modelPath = Path.Combine(pbipFolder, $"{projectName}.SemanticModel", "definition", "model.tmdl");
            if (!File.Exists(modelPath))
                return;

            var sb = new StringBuilder();
            
            // Write the model header
            sb.AppendLine("model Model");
            sb.AppendLine("\tculture: en-US");
            sb.AppendLine("\tdefaultPowerBIDataSourceVersion: powerBI_V3");
            sb.AppendLine("\tsourceQueryCulture: en-US");
            sb.AppendLine("\tdataAccessOptions");
            sb.AppendLine("\t\tlegacyRedirects");
            sb.AppendLine("\t\treturnErrorValuesAsNull");
            sb.AppendLine();
            sb.AppendLine("annotation __PBI_TimeIntelligenceEnabled = 0");
            sb.AppendLine();

            // Build PBI_QueryOrder annotation
            var tableNames = tables.Select(t => t.DisplayName ?? t.LogicalName).ToList();
            tableNames.Insert(0, "DataverseURL"); // DataverseURL is always first
            var queryOrder = string.Join("\",\"", tableNames);
            sb.AppendLine($"annotation PBI_QueryOrder = [\"{queryOrder}\"]");
            sb.AppendLine();
            sb.AppendLine("annotation PBI_ProTooling = [\"TMDLView_Desktop\",\"DevMode\",\"TMDL-Extension\"]");
            sb.AppendLine();

            // Write ref table entries
            foreach (var table in tables)
            {
                var displayName = table.DisplayName ?? table.LogicalName;
                // Use quotes for names with spaces
                if (displayName.Contains(' '))
                {
                    sb.AppendLine($"ref table '{displayName}'");
                }
                else
                {
                    sb.AppendLine($"ref table {displayName}");
                }
            }
            sb.AppendLine();
            sb.AppendLine("ref cultureInfo en-US");
            sb.AppendLine();

            WriteTmdlFile(modelPath, sb.ToString());
        }

        /// <summary>
        /// Copies the PBIP template to the target folder
        /// </summary>
        private void CopyTemplate(string targetFolder, string projectName)
        {
            // Create target folder
            Directory.CreateDirectory(targetFolder);

            // Copy all template files
            CopyDirectory(_templatePath, targetFolder, projectName);
        }

        /// <summary>
        /// Recursively copies a directory, renaming "Template" to the project name
        /// </summary>
        private void CopyDirectory(string sourceDir, string targetDir, string projectName)
        {
            // Create target directory
            Directory.CreateDirectory(targetDir);

            // Copy files
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var fileName = Path.GetFileName(file);
                // Rename files containing "Template"
                var newFileName = fileName.Replace("Template", projectName);
                var targetPath = Path.Combine(targetDir, newFileName);

                // Determine if this is a text file that needs content replacement
                var extension = Path.GetExtension(file).ToLowerInvariant();
                var isTextFile = extension == ".json" || extension == ".pbip" || extension == ".pbism" || 
                                extension == ".pbir" || extension == ".tmdl" || extension == ".txt" || 
                                extension == ".platform";

                if (isTextFile)
                {
                    // Read, replace, and write text files
                    var content = File.ReadAllText(file, Utf8WithoutBom);
                    if (extension == ".json" || extension == ".pbip" || extension == ".pbism" || extension == ".pbir")
                    {
                        content = content.Replace("Template", projectName);
                    }
                    WriteTmdlFile(targetPath, content);
                }
                else
                {
                    // Binary copy for non-text files
                    File.Copy(file, targetPath, true);
                }
            }

            // Copy subdirectories
            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var dirName = Path.GetFileName(dir);
                // Rename directories containing "Template"
                var newDirName = dirName.Replace("Template", projectName);
                CopyDirectory(dir, Path.Combine(targetDir, newDirName), projectName);
            }
        }

        /// <summary>
        /// Updates the project configuration files
        /// </summary>
        private void UpdateProjectConfiguration(string pbipFolder, string projectName, string dataverseUrl)
        {
            // Normalize the Dataverse URL (remove https:// if present)
            var normalizedUrl = dataverseUrl;
            if (normalizedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                normalizedUrl = normalizedUrl.Substring(8);

            // Update expressions.tmdl with the DataverseURL
            var expressionsPath = Path.Combine(pbipFolder, $"{projectName}.SemanticModel", "definition", "expressions.tmdl");
            if (File.Exists(expressionsPath))
            {
                var content = File.ReadAllText(expressionsPath, Utf8WithoutBom);
                // Replace the DataverseURL value
                content = Regex.Replace(
                    content,
                    @"expression DataverseURL = ""[^""]*""",
                    $"expression DataverseURL = \"{normalizedUrl}\""
                );
                WriteTmdlFile(expressionsPath, content);
            }

            // Update .platform file with display name
            var platformPath = Path.Combine(pbipFolder, $"{projectName}.SemanticModel", ".platform");
            if (File.Exists(platformPath))
            {
                try
                {
                    var json = JObject.Parse(File.ReadAllText(platformPath, Utf8WithoutBom));
                    if (json["metadata"] != null)
                    {
                        json["metadata"]!["displayName"] = projectName;
                    }
                    // Generate new logicalId for uniqueness
                    if (json["config"] != null)
                    {
                        json["config"]!["logicalId"] = Guid.NewGuid().ToString();
                    }
                    WriteTmdlFile(platformPath, json.ToString(Formatting.Indented));
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"Warning: Failed to update .platform file: {ex.Message}");
                }
            }

            // Update Report .platform file as well
            var reportPlatformPath = Path.Combine(pbipFolder, $"{projectName}.Report", ".platform");
            if (File.Exists(reportPlatformPath))
            {
                try
                {
                    var json = JObject.Parse(File.ReadAllText(reportPlatformPath, Utf8WithoutBom));
                    if (json["metadata"] != null)
                    {
                        json["metadata"]!["displayName"] = projectName;
                    }
                    // Generate new logicalId for uniqueness
                    if (json["config"] != null)
                    {
                        json["config"]!["logicalId"] = Guid.NewGuid().ToString();
                    }
                    WriteTmdlFile(reportPlatformPath, json.ToString(Formatting.Indented));
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"Warning: Failed to update Report .platform file: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Generates TMDL content for a table
        /// </summary>
        private string GenerateTableTmdl(ExportTable table, Dictionary<string, Dictionary<string, AttributeDisplayInfo>> attributeDisplayInfo, HashSet<string> requiredLookupColumns)
        {
            var sb = new StringBuilder();
            var displayName = table.DisplayName ?? table.SchemaName ?? table.LogicalName;
            var tableLineageTag = Guid.NewGuid().ToString();

            // Add table description if available
            if (!string.IsNullOrEmpty(table.LogicalName))
            {
                sb.AppendLine($"/// Source: {table.LogicalName}");
            }
            sb.AppendLine($"table {QuoteTmdlName(displayName)}");
            sb.AppendLine($"\tlineageTag: {tableLineageTag}");
            sb.AppendLine();

            // Collect columns and SQL fields
            var columns = new List<ColumnInfo>();
            var sqlFields = new List<string>();
            var processedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Get attribute info for this table
            var attrInfo = attributeDisplayInfo.ContainsKey(table.LogicalName) 
                ? attributeDisplayInfo[table.LogicalName] 
                : new Dictionary<string, AttributeDisplayInfo>();

            // Always include primary key first (required for relationships)
            var primaryKey = table.PrimaryIdAttribute ?? table.LogicalName + "id";
            if (!table.Attributes.Any(a => a.LogicalName.Equals(primaryKey, StringComparison.OrdinalIgnoreCase)))
            {
                columns.Add(new ColumnInfo
                {
                    LogicalName = primaryKey,
                    DisplayName = primaryKey,
                    SourceColumn = primaryKey,
                    IsHidden = true,
                    IsKey = true,
                    Description = "Primary Key",
                    AttributeType = "uniqueidentifier"
                });
                sqlFields.Add($"Base.{primaryKey}");
                processedColumns.Add(primaryKey);
            }

            // Add required lookup columns for relationships (if not in selected attributes)
            foreach (var lookupCol in requiredLookupColumns)
            {
                if (!table.Attributes.Any(a => a.LogicalName.Equals(lookupCol, StringComparison.OrdinalIgnoreCase)) && !processedColumns.Contains(lookupCol))
                {
                    columns.Add(new ColumnInfo
                    {
                        LogicalName = lookupCol,
                        DisplayName = lookupCol,
                        SourceColumn = lookupCol,
                        IsHidden = true,
                        Description = "Lookup column for relationship",
                        AttributeType = "lookup"
                    });
                    sqlFields.Add($"Base.{lookupCol}");
                    processedColumns.Add(lookupCol);
                }
            }

            foreach (var attr in table.Attributes)
            {
                // Skip if already processed (e.g., primary key was added above)
                if (processedColumns.Contains(attr.LogicalName))
                    continue;

                var attrDisplayInfo = attrInfo.ContainsKey(attr.LogicalName) ? attrInfo[attr.LogicalName] : null;
                var attrType = attr.AttributeType ?? attrDisplayInfo?.AttributeType ?? "";
                var attrDisplayName = attr.DisplayName ?? attrDisplayInfo?.DisplayName ?? attr.SchemaName ?? attr.LogicalName;
                var targets = attr.Targets ?? attrDisplayInfo?.Targets;

                // Skip statecode - we use it in WHERE clause but don't include in model
                if (attr.LogicalName.Equals("statecode", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Check if this is a lookup, choice, or status field
                var isLookup = attrType.Equals("Lookup", StringComparison.OrdinalIgnoreCase) ||
                               attrType.Equals("Owner", StringComparison.OrdinalIgnoreCase) ||
                               attrType.Equals("Customer", StringComparison.OrdinalIgnoreCase);
                var isChoice = attrType.Equals("Picklist", StringComparison.OrdinalIgnoreCase) ||
                               attrType.Equals("State", StringComparison.OrdinalIgnoreCase) ||
                               attrType.Equals("Status", StringComparison.OrdinalIgnoreCase);
                var isBoolean = attrType.Equals("Boolean", StringComparison.OrdinalIgnoreCase);
                var isPrimaryKey = attr.LogicalName == table.PrimaryIdAttribute;
                var isPrimaryName = attr.LogicalName == table.PrimaryNameAttribute;

                // Build description
                var description = BuildDescription(attr.SchemaName ?? attr.LogicalName, attrType, targets);

                if (isLookup)
                {
                    // For lookups: add the ID column (hidden) and the name column (visible)
                    // Hidden ID column
                    columns.Add(new ColumnInfo
                    {
                        LogicalName = attr.LogicalName,
                        DisplayName = attr.LogicalName, // Use logical name as display for hidden column
                        SourceColumn = attr.LogicalName,
                        IsHidden = true,
                        IsKey = isPrimaryKey,
                        Description = $"{attr.SchemaName ?? attr.LogicalName} | Targets: {string.Join(", ", targets ?? new List<string>())}",
                        AttributeType = "lookup"
                    });
                    sqlFields.Add($"Base.{attr.LogicalName}");

                    // Visible name column
                    var nameColumn = attr.LogicalName + "name";
                    columns.Add(new ColumnInfo
                    {
                        LogicalName = nameColumn,
                        DisplayName = attrDisplayName,
                        SourceColumn = nameColumn,
                        IsHidden = false,
                        IsRowLabel = isPrimaryName,
                        Description = $"{attr.SchemaName ?? attr.LogicalName} | Targets: {string.Join(", ", targets ?? new List<string>())}",
                        AttributeType = "string"  // Name columns are always strings
                    });
                    sqlFields.Add($"Base.{nameColumn}");
                }
                else if (isChoice || isBoolean)
                {
                    // For Choice/Boolean: use fieldname with "name" appended
                    var nameColumn = attr.LogicalName + "name";
                    columns.Add(new ColumnInfo
                    {
                        LogicalName = nameColumn,
                        DisplayName = attrDisplayName,
                        SourceColumn = nameColumn,
                        IsHidden = false,
                        IsRowLabel = isPrimaryName,
                        Description = description,
                        AttributeType = "string"  // Choice/Boolean name columns are always strings
                    });
                    sqlFields.Add($"Base.{nameColumn}");
                }
                else
                {
                    // Regular column
                    columns.Add(new ColumnInfo
                    {
                        LogicalName = attr.LogicalName,
                        DisplayName = isPrimaryKey ? attr.LogicalName : attrDisplayName,
                        SourceColumn = attr.LogicalName,
                        IsHidden = isPrimaryKey,
                        IsKey = isPrimaryKey,
                        IsRowLabel = isPrimaryName,
                        Description = description,
                        AttributeType = attrType
                    });
                    sqlFields.Add($"Base.{attr.LogicalName}");
                }
            }

            // Write columns
            foreach (var col in columns)
            {
                // Add column description using logical name
                if (!string.IsNullOrEmpty(col.LogicalName))
                {
                    sb.AppendLine($"\t/// {col.LogicalName}");
                }

                // Map the data type
                var (dataType, formatString, sourceProviderType, summarizeBy) = MapDataType(col.AttributeType);

                sb.AppendLine($"\tcolumn {QuoteTmdlName(col.DisplayName)}");
                sb.AppendLine($"\t\tdataType: {dataType}");
                if (formatString != null)
                {
                    sb.AppendLine($"\t\tformatString: {formatString}");
                }
                if (sourceProviderType != null)
                {
                    sb.AppendLine($"\t\tsourceProviderType: {sourceProviderType}");
                }
                if (col.IsHidden)
                {
                    sb.AppendLine($"\t\tisHidden");
                }
                if (col.IsKey)
                {
                    sb.AppendLine($"\t\tisKey");
                }
                sb.AppendLine($"\t\tlineageTag: {Guid.NewGuid()}");
                if (col.IsRowLabel)
                {
                    sb.AppendLine($"\t\tisDefaultLabel");
                }
                sb.AppendLine($"\t\tsummarizeBy: {summarizeBy}");
                sb.AppendLine($"\t\tsourceColumn: {col.SourceColumn}");
                sb.AppendLine();
                sb.AppendLine($"\t\tannotation SummarizationSetBy = Automatic");
                sb.AppendLine();
            }

            // Write partition (Power Query)
            var schemaName = table.SchemaName ?? table.LogicalName;

            // Build SQL SELECT list with proper formatting
            var sqlSelectList = new StringBuilder();
            for (int i = 0; i < sqlFields.Count; i++)
            {
                if (i == 0)
                {
                    sqlSelectList.Append($"SELECT {sqlFields[i]}");
                }
                else
                {
                    sqlSelectList.Append($"\n\t\t\t\t        ,{sqlFields[i]}");
                }
            }

            sb.AppendLine($"\tpartition {QuoteTmdlName(displayName)} = m");
            sb.AppendLine($"\t\tmode: directQuery");
            sb.AppendLine($"\t\tsource =");
            sb.AppendLine($"\t\t\t\tlet");
            sb.AppendLine($"\t\t\t\t    Dataverse = CommonDataService.Database(DataverseURL,[CreateNavigationProperties=false]),");
            sb.AppendLine($"\t\t\t\t    Source = Value.NativeQuery(Dataverse,\"");
            sb.AppendLine($"\t\t\t\t");
            sb.AppendLine($"\t\t\t\t    {sqlSelectList}");
            sb.AppendLine($"\t\t\t\t    FROM {schemaName} as Base");
            if (table.HasStateCode)
            {
                sb.AppendLine($"\t\t\t\t    WHERE Base.statecode = 0");
            }
            sb.AppendLine($"\t\t\t\t");
            sb.AppendLine($"\t\t\t\t    \" ,null ,[EnableFolding=true])");
            sb.AppendLine($"\t\t\t\tin");
            sb.AppendLine($"\t\t\t\t    Source");
            sb.AppendLine();
            sb.AppendLine($"\tannotation PBI_NavigationStepName = Navigation");
            sb.AppendLine();
            sb.AppendLine($"\tannotation PBI_ResultType = Table");
            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Generates relationships TMDL content
        /// </summary>
        private string GenerateRelationshipsTmdl(
            List<ExportTable> tables,
            List<ExportRelationship> relationships,
            Dictionary<string, Dictionary<string, AttributeDisplayInfo>> attributeDisplayInfo)
        {
            var sb = new StringBuilder();

            // Create lookup for table display names
            var tableDisplayNames = tables.ToDictionary(
                t => t.LogicalName,
                t => t.DisplayName ?? t.SchemaName ?? t.LogicalName,
                StringComparer.OrdinalIgnoreCase);

            // Create lookup for primary key attributes
            var tablePrimaryKeys = tables.ToDictionary(
                t => t.LogicalName,
                t => t.PrimaryIdAttribute ?? t.LogicalName + "id",
                StringComparer.OrdinalIgnoreCase);

            foreach (var rel in relationships)
            {
                // Skip if either table is not in the model
                if (!tableDisplayNames.ContainsKey(rel.SourceTable) || !tableDisplayNames.ContainsKey(rel.TargetTable))
                    continue;

                var sourceTableDisplay = tableDisplayNames[rel.SourceTable];
                var targetTableDisplay = tableDisplayNames[rel.TargetTable];
                var targetPrimaryKey = tablePrimaryKeys[rel.TargetTable];

                // The source attribute (lookup column) references the target's primary key
                var sourceColumn = rel.SourceAttribute;
                var targetColumn = targetPrimaryKey;

                sb.AppendLine($"relationship {Guid.NewGuid()}");
                
                // Add relyOnReferentialIntegrity for snowflake relationships
                if (rel.IsSnowflake)
                {
                    sb.AppendLine($"\trelyOnReferentialIntegrity");
                }

                // Set inactive if not active
                if (!rel.IsActive)
                {
                    sb.AppendLine($"\tisActive: false");
                }

                sb.AppendLine($"\tfromColumn: {QuoteTmdlName(sourceTableDisplay)}.{sourceColumn}");
                sb.AppendLine($"\ttoColumn: {QuoteTmdlName(targetTableDisplay)}.{targetColumn}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Builds a description string for a column
        /// </summary>
        private string BuildDescription(string schemaName, string attrType, List<string>? targets)
        {
            var parts = new List<string> { schemaName };

            if (targets != null && targets.Any())
            {
                parts.Add($"Targets: {string.Join(", ", targets)}");
            }

            return string.Join(" | ", parts);
        }

        /// <summary>
        /// Maps Dataverse attribute types to Power BI data types
        /// </summary>
        private (string dataType, string? formatString, string? sourceProviderType, string summarizeBy) MapDataType(string? attributeType)
        {
            if (string.IsNullOrEmpty(attributeType))
                return ("string", null, null, "none");

            return attributeType.ToLowerInvariant() switch
            {
                // Numeric types
                "integer" => ("int64", "0", "int", "sum"),
                "bigint" => ("int64", "0", "bigint", "sum"),
                "decimal" => ("decimal", "#,0.00", "decimal", "sum"),
                "double" => ("double", "#,0.00", "float", "sum"),
                "money" => ("decimal", "\\$#,0.00;(\\$#,0.00);\\$#,0.00", "money", "sum"),
                
                // Date/Time types
                "datetime" => ("dateTime", "General Date", "datetime2", "none"),
                
                // Boolean types
                "boolean" => ("boolean", null, "bit", "none"),
                
                // Text types (including Lookup/Choice which use 'name' suffix)
                "string" or "memo" or "lookup" or "owner" or "customer" or 
                "picklist" or "state" or "status" or "uniqueidentifier" => ("string", null, null, "none"),
                
                _ => ("string", null, null, "none")
            };
        }

        /// <summary>
        /// Sanitizes a string to be used as a file name
        /// </summary>
        private string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Join("", name.Select(c => invalid.Contains(c) ? '_' : c));
        }

        /// <summary>
        /// Quotes a TMDL name if it contains spaces or special characters
        /// </summary>
        private string QuoteTmdlName(string name)
        {
            // TMDL requires quotes for names with spaces, special characters, or starting with numbers
            if (name.Contains(' ') || name.Contains('-') || name.Contains('.') || 
                name.Contains('[') || name.Contains(']') || name.Contains('(') || 
                name.Contains(')') || char.IsDigit(name[0]))
            {
                return $"'{name}'";
            }
            return name;
        }

        /// <summary>
        /// Helper class for column information
        /// </summary>
        private class ColumnInfo
        {
            public string LogicalName { get; set; } = "";
            public string DisplayName { get; set; } = "";
            public string SourceColumn { get; set; } = "";
            public bool IsHidden { get; set; }
            public string? Description { get; set; }
            public string? AttributeType { get; set; }  // Dataverse attribute type for data type mapping
            public bool IsKey { get; set; }  // Marks this as the key column (Primary ID)
            public bool IsRowLabel { get; set; }  // Marks this as the row label (Primary Name)
        }
    }
}
