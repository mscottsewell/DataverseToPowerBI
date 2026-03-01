// ===================================================================================
// AdditionalTableSelectorForm.cs - Advanced Table Selection for XrmToolBox
// ===================================================================================
//
// PURPOSE:
// This dialog allows advanced users to include tables in the Power BI semantic
// model that are not auto-discovered through the standard star-schema wizard.
// Users can also discover and select relationships between these tables and the
// existing tables already in the model.
//
// USE CASE:
// The standard FactDimensionSelectorForm discovers dimension tables automatically
// by following lookup fields from the chosen fact table. This dialog addresses
// scenarios where users need additional tables that:
//   1. Are not reachable via the fact table's lookups
//   2. Require relationship columns to connect to the existing star schema
//
// WORKFLOW:
//   1. User checks tables from the full list (tables already in the model are excluded)
//   2. Clicks "Discover Relationships..." to auto-detect relationships between
//      the selected tables and model tables, then picks which ones to include
//   3. Optionally clicks "Add Manually..." to define a custom relationship by hand
//   4. Clicks OK - the selected tables and relationships are returned to the caller
//
// OUTPUT:
//   - SelectedAdditionalTables: List<TableInfo> of newly selected tables
//   - SelectedAdditionalRelationships: List<ExportRelationship> of selected/defined relationships
//
// ===================================================================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Xrm.Sdk;
using DataverseToPowerBI.Core.Models;

namespace DataverseToPowerBI.XrmToolBox
{
    /// <summary>
    /// Dialog for selecting additional tables and defining manual relationships,
    /// beyond those auto-discovered by the star-schema wizard.
    /// </summary>
    public class AdditionalTableSelectorForm : Form
    {
        #region Fields

        private readonly Dictionary<string, string> _allEntityDisplayNames;
        private readonly List<TableInfo> _solutionTables;
        private readonly HashSet<string> _alreadySelectedTableNames;

        // Optional adapter for relationship discovery
        private readonly XrmServiceAdapterImpl? _adapter;
        private readonly IOrganizationService? _service;

        // All available tables (union of solution tables + all entity display names)
        private List<TableInfo> _availableTables = new List<TableInfo>();
        private List<ListViewItem> _allTableItems = new List<ListViewItem>();

        // UI – table selection
        private Label lblTablesHeader = null!;
        private Label lblTablesHint = null!;
        private TextBox txtTableSearch = null!;
        private ListView listViewTables = null!;

        // UI – relationship definitions
        private Label lblRelHeader = null!;
        private Label lblRelHint = null!;
        private ListView listViewRelationships = null!;
        private Button btnDiscoverRelationships = null!;
        private Button btnAddRelationship = null!;
        private Button btnRemoveRelationship = null!;

        // UI – footer
        private Button btnOK = null!;
        private Button btnCancel = null!;

        private bool _suppressItemCheckedEvent = false;

        // Cached bold font – disposed in Dispose()
        private Font? _boldFont;

        #endregion

        #region Output Properties

        /// <summary>Tables the user selected to add to the model.</summary>
        public List<TableInfo> SelectedAdditionalTables { get; private set; } = new List<TableInfo>();

        /// <summary>Manual relationships the user defined between selected tables.</summary>
        public List<ExportRelationship> SelectedAdditionalRelationships { get; private set; } = new List<ExportRelationship>();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the dialog.
        /// </summary>
        /// <param name="allEntityDisplayNames">All entity logical-name → display-name pairs (from Dataverse metadata).</param>
        /// <param name="solutionTables">Tables in the currently-selected solution (for richer TableInfo objects).</param>
        /// <param name="alreadySelectedTableNames">Logical names of tables already in the star-schema model (fact + dimensions + snowflakes).</param>
        /// <param name="currentAdditionalTables">Previously selected additional tables (for edit scenarios).</param>
        /// <param name="currentAdditionalRelationships">Previously defined relationships (for edit scenarios).</param>
        /// <param name="adapter">Optional adapter for auto-discovering relationships from Dataverse metadata.</param>
        /// <param name="service">Optional organization service required when <paramref name="adapter"/> is provided.</param>
        public AdditionalTableSelectorForm(
            Dictionary<string, string> allEntityDisplayNames,
            List<TableInfo> solutionTables,
            IEnumerable<string> alreadySelectedTableNames,
            List<TableInfo>? currentAdditionalTables = null,
            List<ExportRelationship>? currentAdditionalRelationships = null,
            XrmServiceAdapterImpl? adapter = null,
            IOrganizationService? service = null)
        {
            _allEntityDisplayNames = allEntityDisplayNames ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _solutionTables = solutionTables ?? new List<TableInfo>();
            _alreadySelectedTableNames = new HashSet<string>(
                alreadySelectedTableNames ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);
            _adapter = adapter;
            _service = service;

            _boldFont = new Font(this.Font, FontStyle.Bold);
            _availableTables = BuildAvailableTables();

            InitializeComponent();
            PopulateTableList(currentAdditionalTables ?? new List<TableInfo>());
            PopulateRelationshipList(currentAdditionalRelationships ?? new List<ExportRelationship>());
        }

        #endregion

        #region Available-table building

        /// <summary>
        /// Builds the full set of tables the user can pick from:
        /// solution tables (with rich metadata) unioned with all entity display names
        /// (providing coverage for tables outside the current solution).
        /// Tables already present in the star-schema model are excluded.
        /// </summary>
        private List<TableInfo> BuildAvailableTables()
        {
            var byLogicalName = new Dictionary<string, TableInfo>(StringComparer.OrdinalIgnoreCase);

            // Start with solution tables (richer metadata)
            foreach (var t in _solutionTables)
                byLogicalName[t.LogicalName] = t;

            // Fill in any table known from display-name map that isn't already covered
            foreach (var kvp in _allEntityDisplayNames)
            {
                if (!byLogicalName.ContainsKey(kvp.Key))
                {
                    byLogicalName[kvp.Key] = new TableInfo
                    {
                        LogicalName = kvp.Key,
                        DisplayName = kvp.Value,
                        SchemaName = kvp.Key,
                        PrimaryIdAttribute = kvp.Key + "id",
                        PrimaryNameAttribute = "name"
                    };
                }
            }

            // Exclude tables that are already part of the star schema
            return byLogicalName.Values
                .Where(t => !_alreadySelectedTableNames.Contains(t.LogicalName))
                .OrderBy(t => t.DisplayName ?? t.LogicalName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        #endregion

        #region InitializeComponent

        private void InitializeComponent()
        {
            this.Text = "Add Tables to Model";
            this.Width = 870;
            this.Height = 700;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(700, 550);

            // ── Table-selection section ─────────────────────────────────────────
            lblTablesHeader = new Label
            {
                Text = "Additional Tables:",
                Font = _boldFont,
                Location = new Point(10, 10),
                AutoSize = true
            };
            this.Controls.Add(lblTablesHeader);

            lblTablesHint = new Label
            {
                Text = "Select tables to add to the model. " +
                       "Tables already included via the star-schema wizard are excluded.",
                Location = new Point(10, 30),
                Size = new Size(840, 18),
                ForeColor = Color.DimGray
            };
            this.Controls.Add(lblTablesHint);

            var lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(10, 58),
                AutoSize = true
            };
            this.Controls.Add(lblSearch);

            txtTableSearch = new TextBox
            {
                Location = new Point(65, 55),
                Width = 270
            };
            txtTableSearch.TextChanged += (s, e) => FilterTableList();
            this.Controls.Add(txtTableSearch);

            listViewTables = new ListView
            {
                Location = new Point(10, 85),
                Size = new Size(840, 215),
                View = View.Details,
                CheckBoxes = true,
                FullRowSelect = true,
                GridLines = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            listViewTables.Columns.Add("Display Name", 320);
            listViewTables.Columns.Add("Logical Name", 240);
            listViewTables.Columns.Add("Schema Name", 230);
            listViewTables.ItemChecked += ListViewTables_ItemChecked;
            this.Controls.Add(listViewTables);

            // ── Relationship-definition section ────────────────────────────────
            lblRelHeader = new Label
            {
                Text = "Relationships (optional):",
                Font = _boldFont,
                Location = new Point(10, 312),
                AutoSize = true
            };
            this.Controls.Add(lblRelHeader);

            lblRelHint = new Label
            {
                Text = "Click 'Discover Relationships...' to auto-detect relationships between selected and model tables, " +
                       "or 'Add Manually...' to define a custom relationship.",
                Location = new Point(10, 332),
                Size = new Size(840, 18),
                ForeColor = Color.DimGray
            };
            this.Controls.Add(lblRelHint);

            listViewRelationships = new ListView
            {
                Location = new Point(10, 358),
                Size = new Size(840, 215),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
            };
            listViewRelationships.Columns.Add("Source Table (many side)", 230);
            listViewRelationships.Columns.Add("Lookup Attribute", 200);
            listViewRelationships.Columns.Add("Target Table (one side)", 230);
            listViewRelationships.Columns.Add("Active", 60);
            this.Controls.Add(listViewRelationships);

            btnDiscoverRelationships = new Button
            {
                Text = "Discover Relationships...",
                Location = new Point(10, 583),
                Size = new Size(185, 28),
                Enabled = _adapter != null && _service != null
            };
            btnDiscoverRelationships.Click += BtnDiscoverRelationships_Click;
            this.Controls.Add(btnDiscoverRelationships);

            btnAddRelationship = new Button
            {
                Text = "Add Manually...",
                Location = new Point(202, 583),
                Size = new Size(128, 28)
            };
            btnAddRelationship.Click += BtnAddRelationship_Click;
            this.Controls.Add(btnAddRelationship);

            btnRemoveRelationship = new Button
            {
                Text = "Remove Selected",
                Location = new Point(337, 583),
                Size = new Size(128, 28)
            };
            btnRemoveRelationship.Click += BtnRemoveRelationship_Click;
            this.Controls.Add(btnRemoveRelationship);

            // ── Footer ─────────────────────────────────────────────────────────
            btnOK = new Button
            {
                Text = "OK",
                Location = new Point(708, 622),
                Size = new Size(70, 28),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            btnOK.Click += BtnOK_Click;
            this.Controls.Add(btnOK);

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(785, 622),
                Size = new Size(70, 28),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }

        #endregion

        #region Table-list population and filtering

        private void PopulateTableList(List<TableInfo> currentAdditionalTables)
        {
            _suppressItemCheckedEvent = true;
            listViewTables.BeginUpdate();
            listViewTables.Items.Clear();
            _allTableItems.Clear();

            var preChecked = new HashSet<string>(
                currentAdditionalTables.Select(t => t.LogicalName),
                StringComparer.OrdinalIgnoreCase);

            foreach (var table in _availableTables)
            {
                var item = new ListViewItem(table.DisplayName ?? table.LogicalName);
                item.SubItems.Add(table.LogicalName);
                item.SubItems.Add(table.SchemaName ?? "");
                item.Tag = table;
                item.Checked = preChecked.Contains(table.LogicalName);

                listViewTables.Items.Add(item);
                _allTableItems.Add(item);
            }

            listViewTables.EndUpdate();
            _suppressItemCheckedEvent = false;
        }

        private void FilterTableList()
        {
            var search = txtTableSearch.Text.Trim().ToLowerInvariant();

            listViewTables.BeginUpdate();
            listViewTables.Items.Clear();

            foreach (var item in _allTableItems)
            {
                if (string.IsNullOrEmpty(search))
                {
                    listViewTables.Items.Add(item);
                    continue;
                }

                var table = item.Tag as TableInfo;
                if (table == null) continue;

                var matches =
                    (table.DisplayName?.ToLowerInvariant().Contains(search) == true) ||
                    table.LogicalName.ToLowerInvariant().Contains(search);

                if (matches)
                    listViewTables.Items.Add(item);
            }

            listViewTables.EndUpdate();
        }

        private void ListViewTables_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (_suppressItemCheckedEvent) return;
            // No special action needed here – selection is read in BtnOK_Click.
        }

        #endregion

        #region Relationship-list population

        private void PopulateRelationshipList(List<ExportRelationship> currentRelationships)
        {
            listViewRelationships.Items.Clear();

            foreach (var rel in currentRelationships)
                AddRelationshipRow(rel);
        }

        private void AddRelationshipRow(ExportRelationship rel)
        {
            var sourceDisplay = ResolveDisplayName(rel.SourceTable);
            var targetDisplay = ResolveDisplayName(rel.TargetTable);

            var item = new ListViewItem(sourceDisplay);
            item.SubItems.Add(rel.SourceAttribute);
            item.SubItems.Add(targetDisplay);
            item.SubItems.Add(rel.IsActive ? "Yes" : "No");
            item.Tag = rel;
            listViewRelationships.Items.Add(item);
        }

        private string ResolveDisplayName(string logicalName)
        {
            var t = _solutionTables.FirstOrDefault(x =>
                x.LogicalName.Equals(logicalName, StringComparison.OrdinalIgnoreCase));
            if (t?.DisplayName != null)
                return t.DisplayName;

            if (_allEntityDisplayNames.TryGetValue(logicalName, out var dn))
                return dn;

            return logicalName;
        }

        #endregion

        #region Button handlers

        private void BtnAddRelationship_Click(object sender, EventArgs e)
        {
            // Source/target tables = already-in-model tables + currently-checked additional tables
            var checkedAdditional = _allTableItems
                .Where(i => i.Checked && i.Tag is TableInfo)
                .Select(i => (TableInfo)i.Tag)
                .ToList();

            var allTablesForRel = new List<TableInfo>();

            foreach (var name in _alreadySelectedTableNames)
            {
                var display = ResolveDisplayName(name);
                allTablesForRel.Add(new TableInfo { LogicalName = name, DisplayName = display });
            }

            foreach (var t in checkedAdditional)
            {
                if (!allTablesForRel.Any(x => x.LogicalName.Equals(t.LogicalName, StringComparison.OrdinalIgnoreCase)))
                    allTablesForRel.Add(t);
            }

            if (allTablesForRel.Count < 2)
            {
                MessageBox.Show(
                    "You need at least two tables to define a relationship.\n\n" +
                    "Select additional tables above, and make sure there are tables already in the model.",
                    "Not Enough Tables",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new AddRelationshipDialog(allTablesForRel))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
                    AddRelationshipRow(dlg.Result);
            }
        }

        private void BtnRemoveRelationship_Click(object sender, EventArgs e)
        {
            if (listViewRelationships.SelectedItems.Count == 0) return;

            foreach (ListViewItem item in listViewRelationships.SelectedItems.Cast<ListViewItem>().ToList())
                listViewRelationships.Items.Remove(item);
        }

        private void BtnDiscoverRelationships_Click(object sender, EventArgs e)
        {
            if (_adapter == null || _service == null) return;

            var checkedAdditional = _allTableItems
                .Where(i => i.Checked && i.Tag is TableInfo)
                .Select(i => (TableInfo)i.Tag)
                .ToList();

            if (!checkedAdditional.Any())
            {
                MessageBox.Show(
                    "Select at least one additional table above before discovering relationships.",
                    "No Tables Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var allModelTableNames = new HashSet<string>(_alreadySelectedTableNames, StringComparer.OrdinalIgnoreCase);
            // Include other checked additional tables as valid relationship partners too
            var checkedAdditionalTableNames = checkedAdditional
                .Select(t => t.LogicalName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var discovered = new List<DiscoveredRelationshipItem>();
            var originalCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                foreach (var table in checkedAdditional)
                {
                    try
                    {
                        // ManyToOne: additional table → model/other-selected tables (lookup fields on this table)
                        var attrs = _adapter.GetAttributesSync(_service, table.LogicalName);
                        var lookups = attrs
                            .Where(a => a.AttributeType == "Lookup" && a.Targets != null && a.Targets.Any());

                        foreach (var lookup in lookups)
                        {
                            foreach (var target in lookup.Targets!)
                            {
                                if (!allModelTableNames.Contains(target) && !checkedAdditionalTableNames.Contains(target))
                                    continue;
                                if (target.Equals(table.LogicalName, StringComparison.OrdinalIgnoreCase))
                                    continue;

                                discovered.Add(new DiscoveredRelationshipItem
                                {
                                    SourceTable = table.LogicalName,
                                    SourceTableDisplayName = table.DisplayName ?? table.LogicalName,
                                    SourceAttribute = lookup.LogicalName,
                                    LookupDisplayName = lookup.DisplayName ?? lookup.LogicalName,
                                    TargetTable = target,
                                    TargetTableDisplayName = ResolveDisplayName(target)
                                });
                            }
                        }

                        // OneToMany: model/other-selected tables → additional table (lookup on the model table side)
                        var oneToMany = _adapter.GetOneToManyRelationshipsSync(_service, table.LogicalName);
                        foreach (var rel in oneToMany)
                        {
                            if (!allModelTableNames.Contains(rel.ReferencingEntity) &&
                                !checkedAdditionalTableNames.Contains(rel.ReferencingEntity))
                                continue;

                            discovered.Add(new DiscoveredRelationshipItem
                            {
                                SourceTable = rel.ReferencingEntity,
                                SourceTableDisplayName = ResolveDisplayName(rel.ReferencingEntity),
                                SourceAttribute = rel.ReferencingAttribute,
                                LookupDisplayName = rel.LookupDisplayName ?? rel.ReferencingAttribute,
                                TargetTable = table.LogicalName,
                                TargetTableDisplayName = table.DisplayName ?? table.LogicalName
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Services.DebugLogger.Log($"Error discovering relationships for {table.LogicalName}: {ex.Message}");
                    }
                }
            }
            finally
            {
                this.Cursor = originalCursor;
            }

            // Deduplicate (normalize keys once per item)
            var seenKeys = new HashSet<(string, string, string)>();
            var deduped = new List<DiscoveredRelationshipItem>(discovered.Count);
            foreach (var d in discovered)
            {
                if (seenKeys.Add(d.GetNormalizedKey()))
                    deduped.Add(d);
            }
            discovered = deduped;

            // Remove relationships already present in the list (build key set once)
            var existingKeys = new HashSet<(string, string, string)>();
            foreach (ListViewItem lvi in listViewRelationships.Items)
            {
                if (lvi.Tag is ExportRelationship r)
                    existingKeys.Add((r.SourceTable.ToLowerInvariant(), r.SourceAttribute.ToLowerInvariant(), r.TargetTable.ToLowerInvariant()));
            }

            discovered = discovered
                .Where(d => !existingKeys.Contains(d.GetNormalizedKey()))
                .ToList();

            if (!discovered.Any())
            {
                MessageBox.Show(
                    "No new relationships were found between the selected tables and the model tables.\n\n" +
                    "Use 'Add Manually...' to define a custom relationship.",
                    "No Relationships Found",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new DiscoverRelationshipsDialog(discovered))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    foreach (var rel in dlg.SelectedRelationships)
                        AddRelationshipRow(rel);
                }
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            // Collect checked tables
            SelectedAdditionalTables = _allTableItems
                .Where(i => i.Checked && i.Tag is TableInfo)
                .Select(i => (TableInfo)i.Tag)
                .ToList();

            // Collect defined relationships
            SelectedAdditionalRelationships = listViewRelationships.Items
                .Cast<ListViewItem>()
                .Where(i => i.Tag is ExportRelationship)
                .Select(i => (ExportRelationship)i.Tag)
                .ToList();

            // Validate – orphaned relationships reference tables not in the combined selection
            var allNames = new HashSet<string>(
                _alreadySelectedTableNames.Concat(SelectedAdditionalTables.Select(t => t.LogicalName)),
                StringComparer.OrdinalIgnoreCase);

            var orphaned = SelectedAdditionalRelationships
                .Where(r => !allNames.Contains(r.SourceTable) || !allNames.Contains(r.TargetTable))
                .ToList();

            if (orphaned.Any())
            {
                var detail = string.Join("\n", orphaned.Select(r =>
                    $"  {ResolveDisplayName(r.SourceTable)} → {ResolveDisplayName(r.TargetTable)} ({r.SourceAttribute})"));

                var answer = MessageBox.Show(
                    $"The following relationships reference tables that are not selected:\n\n{detail}\n\n" +
                    "These relationships will be removed. Continue?",
                    "Orphaned Relationships",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (answer != DialogResult.Yes)
                    return;

                SelectedAdditionalRelationships = SelectedAdditionalRelationships
                    .Except(orphaned)
                    .ToList();
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        #endregion

        #region Dispose

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _boldFont?.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        #region Nested: DiscoveredRelationshipItem

        /// <summary>
        /// Represents a relationship discovered from Dataverse metadata.
        /// </summary>
        private class DiscoveredRelationshipItem
        {
            public string SourceTable { get; set; } = "";
            public string SourceTableDisplayName { get; set; } = "";
            public string SourceAttribute { get; set; } = "";
            public string LookupDisplayName { get; set; } = "";
            public string TargetTable { get; set; } = "";
            public string TargetTableDisplayName { get; set; } = "";

            /// <summary>Returns a lowercased tuple for case-insensitive deduplication and filtering.</summary>
            public (string, string, string) GetNormalizedKey() =>
                (SourceTable.ToLowerInvariant(), SourceAttribute.ToLowerInvariant(), TargetTable.ToLowerInvariant());
        }

        #endregion

        #region Nested: DiscoverRelationshipsDialog

        /// <summary>
        /// Dialog showing auto-discovered relationships from Dataverse metadata.
        /// Users check the relationships they want to include and optionally toggle Active/Inactive.
        /// </summary>
        private class DiscoverRelationshipsDialog : Form
        {
            private ListView listViewDiscovered = null!;
            private Button btnSelectAll = null!;
            private Button btnClearAll = null!;
            private Button btnToggleActive = null!;
            private Button btnAdd = null!;
            private Button btnCancel = null!;

            /// <summary>Relationships the user chose to include.</summary>
            public List<ExportRelationship> SelectedRelationships { get; private set; } = new List<ExportRelationship>();

            public DiscoverRelationshipsDialog(List<DiscoveredRelationshipItem> items)
            {
                InitializeComponent();
                PopulateList(items);
            }

            private void InitializeComponent()
            {
                this.Text = "Discovered Relationships";
                this.ClientSize = new Size(780, 480);
                this.StartPosition = FormStartPosition.CenterParent;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.MinimumSize = new Size(600, 400);
                this.MaximizeBox = false;

                var lblHint = new Label
                {
                    Text = "The following relationships were found between the selected tables and existing model tables.\r\n" +
                           "Check the ones you want to include. Double-click a row to toggle Active/Inactive.",
                    Location = new Point(10, 10),
                    Size = new Size(760, 36),
                    ForeColor = Color.DimGray
                };
                this.Controls.Add(lblHint);

                listViewDiscovered = new ListView
                {
                    Location = new Point(10, 55),
                    Size = new Size(760, 340),
                    View = View.Details,
                    CheckBoxes = true,
                    FullRowSelect = true,
                    GridLines = true,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
                };
                listViewDiscovered.Columns.Add("Source Table (many side)", 210);
                listViewDiscovered.Columns.Add("Lookup Field", 200);
                listViewDiscovered.Columns.Add("Target Table (one side)", 210);
                listViewDiscovered.Columns.Add("Active?", 70);
                listViewDiscovered.DoubleClick += ListViewDiscovered_DoubleClick;
                this.Controls.Add(listViewDiscovered);

                int btnY = 407;

                btnSelectAll = new Button { Text = "Select All", Location = new Point(10, btnY), Size = new Size(90, 28), Anchor = AnchorStyles.Left | AnchorStyles.Bottom };
                btnSelectAll.Click += (s, e) => { foreach (ListViewItem i in listViewDiscovered.Items) i.Checked = true; };
                this.Controls.Add(btnSelectAll);

                btnClearAll = new Button { Text = "Clear All", Location = new Point(107, btnY), Size = new Size(90, 28), Anchor = AnchorStyles.Left | AnchorStyles.Bottom };
                btnClearAll.Click += (s, e) => { foreach (ListViewItem i in listViewDiscovered.Items) i.Checked = false; };
                this.Controls.Add(btnClearAll);

                btnToggleActive = new Button { Text = "Toggle Active", Location = new Point(204, btnY), Size = new Size(110, 28), Anchor = AnchorStyles.Left | AnchorStyles.Bottom };
                btnToggleActive.Click += BtnToggleActive_Click;
                this.Controls.Add(btnToggleActive);

                btnAdd = new Button
                {
                    Text = "Add Selected",
                    Location = new Point(588, btnY),
                    Size = new Size(90, 28),
                    Anchor = AnchorStyles.Right | AnchorStyles.Bottom
                };
                btnAdd.Click += BtnAdd_Click;
                this.Controls.Add(btnAdd);

                btnCancel = new Button
                {
                    Text = "Cancel",
                    Location = new Point(685, btnY),
                    Size = new Size(75, 28),
                    DialogResult = DialogResult.Cancel,
                    Anchor = AnchorStyles.Right | AnchorStyles.Bottom
                };
                this.Controls.Add(btnCancel);

                this.AcceptButton = btnAdd;
                this.CancelButton = btnCancel;
            }

            private void PopulateList(List<DiscoveredRelationshipItem> items)
            {
                listViewDiscovered.BeginUpdate();
                foreach (var item in items)
                {
                    var lvi = new ListViewItem(item.SourceTableDisplayName);
                    lvi.SubItems.Add(string.IsNullOrEmpty(item.LookupDisplayName) || item.LookupDisplayName == item.SourceAttribute
                        ? item.SourceAttribute
                        : $"{item.LookupDisplayName} ({item.SourceAttribute})");
                    lvi.SubItems.Add(item.TargetTableDisplayName);
                    lvi.SubItems.Add("Active");
                    // Tag stores the source item + mutable active flag together
                    lvi.Tag = new RelItemState(item, isActive: true);
                    lvi.Checked = true;
                    listViewDiscovered.Items.Add(lvi);
                }
                listViewDiscovered.EndUpdate();
            }

            private void ListViewDiscovered_DoubleClick(object sender, EventArgs e)
            {
                if (listViewDiscovered.SelectedItems.Count == 0) return;
                ToggleActiveForItems(listViewDiscovered.SelectedItems.Cast<ListViewItem>().ToList());
            }

            private void BtnToggleActive_Click(object sender, EventArgs e)
            {
                var targets = listViewDiscovered.CheckedItems.Cast<ListViewItem>().ToList();
                if (!targets.Any())
                {
                    MessageBox.Show("Check at least one relationship to toggle its active state.", "Nothing Checked",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                ToggleActiveForItems(targets);
            }

            private void ToggleActiveForItems(List<ListViewItem> items)
            {
                foreach (var lvi in items)
                {
                    if (lvi.Tag is RelItemState state)
                    {
                        state.IsActive = !state.IsActive;
                        lvi.SubItems[3].Text = state.IsActive ? "Active" : "Inactive";
                    }
                }
            }

            private void BtnAdd_Click(object sender, EventArgs e)
            {
                var selected = listViewDiscovered.CheckedItems.Cast<ListViewItem>().ToList();
                if (!selected.Any())
                {
                    MessageBox.Show("Check at least one relationship to add.", "Nothing Selected",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                SelectedRelationships = new List<ExportRelationship>();
                foreach (var lvi in selected)
                {
                    if (lvi.Tag is RelItemState state)
                    {
                        SelectedRelationships.Add(new ExportRelationship
                        {
                            SourceTable = state.Item.SourceTable,
                            SourceAttribute = state.Item.SourceAttribute,
                            TargetTable = state.Item.TargetTable,
                            DisplayName = $"{state.Item.SourceTableDisplayName} → {state.Item.TargetTableDisplayName}",
                            IsActive = state.IsActive
                        });
                    }
                }

                DialogResult = DialogResult.OK;
                Close();
            }

            /// <summary>Holds a discovered relationship item and its mutable active state for use as a ListView tag.</summary>
            private class RelItemState
            {
                public DiscoveredRelationshipItem Item { get; }
                public bool IsActive { get; set; }

                public RelItemState(DiscoveredRelationshipItem item, bool isActive)
                {
                    Item = item;
                    IsActive = isActive;
                }
            }
        }

        #endregion

        #region Nested: AddRelationshipDialog

        /// <summary>
        /// Small child dialog for specifying a single many-to-one relationship.
        /// </summary>
        private class AddRelationshipDialog : Form
        {
            private ComboBox cmbSourceTable = null!;
            private TextBox txtLookupAttribute = null!;
            private ComboBox cmbTargetTable = null!;
            private CheckBox chkIsActive = null!;
            private Button btnAdd = null!;
            private Button btnCancel = null!;

            private readonly List<TableInfo> _tables;

            /// <summary>The relationship defined by the user, or <c>null</c> if cancelled.</summary>
            public ExportRelationship? Result { get; private set; }

            public AddRelationshipDialog(List<TableInfo> tables)
            {
                _tables = tables.OrderBy(t => t.DisplayName ?? t.LogicalName, StringComparer.OrdinalIgnoreCase).ToList();
                InitializeComponent();
                PopulateCombos();
            }

            private void InitializeComponent()
            {
                this.Text = "Add Relationship";
                this.ClientSize = new Size(480, 215);
                this.StartPosition = FormStartPosition.CenterParent;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;
                this.MinimizeBox = false;

                int y = 18;
                const int labelW = 175;
                const int comboX = 185;
                const int comboW = 280;
                const int rowH = 38;

                this.Controls.Add(new Label { Text = "Source Table (many side):", Location = new Point(10, y + 4), Width = labelW });
                cmbSourceTable = new ComboBox
                {
                    Location = new Point(comboX, y),
                    Width = comboW,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                this.Controls.Add(cmbSourceTable);
                y += rowH;

                this.Controls.Add(new Label
                {
                    Text = "Lookup Attribute:",
                    Location = new Point(10, y + 4),
                    Width = labelW
                });
                txtLookupAttribute = new TextBox
                {
                    Location = new Point(comboX, y),
                    Width = comboW
                };
                this.Controls.Add(txtLookupAttribute);

                // Hint label below attribute field
                this.Controls.Add(new Label
                {
                    Text = "e.g. _accountid_value",
                    Location = new Point(comboX, y + 24),
                    Width = comboW,
                    ForeColor = Color.Gray
                });
                y += rowH + 16;

                this.Controls.Add(new Label { Text = "Target Table (one side):", Location = new Point(10, y + 4), Width = labelW });
                cmbTargetTable = new ComboBox
                {
                    Location = new Point(comboX, y),
                    Width = comboW,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                this.Controls.Add(cmbTargetTable);
                y += rowH;

                chkIsActive = new CheckBox
                {
                    Text = "Active relationship",
                    Location = new Point(comboX, y),
                    Checked = true,
                    AutoSize = true
                };
                this.Controls.Add(chkIsActive);
                y += 30;

                btnAdd = new Button
                {
                    Text = "Add",
                    Location = new Point(310, y),
                    Size = new Size(75, 28)
                };
                btnAdd.Click += BtnAdd_Click;
                this.Controls.Add(btnAdd);

                btnCancel = new Button
                {
                    Text = "Cancel",
                    Location = new Point(393, y),
                    Size = new Size(75, 28),
                    DialogResult = DialogResult.Cancel
                };
                this.Controls.Add(btnCancel);

                this.AcceptButton = btnAdd;
                this.CancelButton = btnCancel;
            }

            private void PopulateCombos()
            {
                foreach (var t in _tables)
                {
                    var item = new TableComboItem(t);
                    cmbSourceTable.Items.Add(item);
                    cmbTargetTable.Items.Add(item);
                }

                if (cmbSourceTable.Items.Count > 0)
                    cmbSourceTable.SelectedIndex = 0;
                if (cmbTargetTable.Items.Count > 0)
                    cmbTargetTable.SelectedIndex = Math.Min(1, cmbTargetTable.Items.Count - 1);
            }

            private void BtnAdd_Click(object sender, EventArgs e)
            {
                var sourceItem = cmbSourceTable.SelectedItem as TableComboItem;
                var targetItem = cmbTargetTable.SelectedItem as TableComboItem;
                var attr = txtLookupAttribute.Text.Trim();

                if (sourceItem == null || targetItem == null)
                {
                    MessageBox.Show("Please select source and target tables.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(attr))
                {
                    MessageBox.Show("Please enter a lookup attribute logical name.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (sourceItem.Table.LogicalName.Equals(targetItem.Table.LogicalName, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Source and target tables must be different.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var sourceName = sourceItem.Table.DisplayName ?? sourceItem.Table.LogicalName;
                var targetName = targetItem.Table.DisplayName ?? targetItem.Table.LogicalName;

                Result = new ExportRelationship
                {
                    SourceTable = sourceItem.Table.LogicalName,
                    SourceAttribute = attr,
                    TargetTable = targetItem.Table.LogicalName,
                    DisplayName = $"{sourceName} → {targetName}",
                    IsActive = chkIsActive.Checked
                };

                DialogResult = DialogResult.OK;
                Close();
            }

            private class TableComboItem
            {
                public TableInfo Table { get; }

                public TableComboItem(TableInfo table) => Table = table;

                public override string ToString() =>
                    string.IsNullOrEmpty(Table.DisplayName) || Table.DisplayName == Table.LogicalName
                        ? Table.LogicalName
                        : $"{Table.DisplayName} ({Table.LogicalName})";
            }
        }

        #endregion
    }
}
