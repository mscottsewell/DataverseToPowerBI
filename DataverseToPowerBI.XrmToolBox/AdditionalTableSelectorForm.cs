// ===================================================================================
// AdditionalTableSelectorForm.cs - Advanced Table Selection for XrmToolBox
// ===================================================================================
//
// PURPOSE:
// This dialog allows advanced users to include tables in the Power BI semantic
// model that are not auto-discovered through the standard star-schema wizard.
// Users can also define manual relationships between these tables and the
// existing tables already in the model.
//
// USE CASE:
// The standard FactDimensionSelectorForm discovers dimension tables automatically
// by following lookup fields from the chosen fact table. This dialog addresses
// scenarios where users need additional tables that:
//   1. Are not reachable via the fact table's lookups
//   2. Require manually-specified relationship columns (e.g., cross-entity joins)
//
// WORKFLOW:
//   1. User checks tables from the full list (tables already in the model are excluded)
//   2. Optionally clicks "Add Relationship..." to define manual many-to-one joins
//   3. Clicks OK - the selected tables and relationships are returned to the caller
//
// OUTPUT:
//   - SelectedAdditionalTables: List<TableInfo> of newly selected tables
//   - SelectedAdditionalRelationships: List<ExportRelationship> of manually-defined relationships
//
// ===================================================================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DataverseToPowerBI.Core.Models;
using Microsoft.Xrm.Sdk;
using CoreAttributeMetadata = DataverseToPowerBI.Core.Models.AttributeMetadata;
using WinLabel = System.Windows.Forms.Label;

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

        // Dataverse adapter for relationship auto-discovery (optional; null = manual mode)
        private readonly XrmServiceAdapterImpl? _adapter;
        private readonly IOrganizationService? _service;

        // All available tables (union of solution tables + all entity display names)
        private List<TableInfo> _availableTables = new List<TableInfo>();
        private List<ListViewItem> _allTableItems = new List<ListViewItem>();

        // Authoritative backing set for which additional tables are currently checked.
        // Used instead of item.Checked on _allTableItems to avoid ArgumentOutOfRangeException
        // when items have been filtered out of the ListView (Index == -1).
        private readonly HashSet<string> _checkedAdditionalTableNames =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Backing store for ALL relationship items (visible list is a filtered subset)
        private List<ListViewItem> _allRelationshipItems = new List<ListViewItem>();

        // Attribute/relationship caches to avoid re-querying Dataverse on re-check
        private Dictionary<string, List<CoreAttributeMetadata>> _attributeCache
            = new Dictionary<string, List<CoreAttributeMetadata>>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, List<OneToManyRelationshipInfo>> _oneToManyCache
            = new Dictionary<string, List<OneToManyRelationshipInfo>>(StringComparer.OrdinalIgnoreCase);

        // UI – table selection
        private WinLabel lblTablesHeader = null!;
        private WinLabel lblTablesHint = null!;
        private TextBox txtTableSearch = null!;
        private CheckBox _chkSelectedOnly = null!;
        private Button btnPasteNames = null!;
        private ListView listViewTables = null!;

        // UI – relationship definitions
        private WinLabel lblRelHeader = null!;
        private WinLabel lblRelHint = null!;
        private ListView listViewRelationships = null!;
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
        /// <param name="adapter">Dataverse adapter used for auto-discovering relationships.</param>
        /// <param name="service">Organization service used with <paramref name="adapter"/>.</param>
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
            // Restore previously saved relationships first (tagged DiscoveredForTable=null so they survive unchecks)
            PopulateRelationshipList(currentAdditionalRelationships ?? new List<ExportRelationship>());
            // Auto-discover for any pre-checked tables (edit re-open); skips duplicates already restored above
            LoadDiscoveredRelationshipsForPreCheckedTables();
            UpdateRelationshipFilter();
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
            this.Height = 660;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(700, 550);

            // ── Table-selection section ─────────────────────────────────────────
            lblTablesHeader = new WinLabel
            {
                Text = "Additional Tables:",
                Font = _boldFont,
                Location = new Point(10, 10),
                AutoSize = true
            };
            this.Controls.Add(lblTablesHeader);

            lblTablesHint = new WinLabel
            {
                Text = "Select tables to add to the model. " +
                       "Tables already included via the star-schema wizard are excluded.",
                Location = new Point(10, 30),
                Size = new Size(840, 18),
                ForeColor = Color.DimGray
            };
            this.Controls.Add(lblTablesHint);

            var lblSearch = new WinLabel
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

            _chkSelectedOnly = new CheckBox
            {
                Text = "Selected only",
                Location = new Point(350, 57),
                AutoSize = true
            };
            _chkSelectedOnly.CheckedChanged += (s, e) => FilterTableList();
            this.Controls.Add(_chkSelectedOnly);

            btnPasteNames = new Button
            {
                Text = "Paste Names...",
                Location = new Point(730, 53),
                Size = new Size(110, 24),
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            btnPasteNames.Click += BtnPasteNames_Click;
            this.Controls.Add(btnPasteNames);

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
            listViewTables.SelectedIndexChanged += (s, e) => UpdateRelationshipFilter();
            this.Controls.Add(listViewTables);

            // ── Relationship-definition section ────────────────────────────────
            lblRelHeader = new WinLabel
            {
                Text = "Relationships:",
                Font = _boldFont,
                Location = new Point(10, 312),
                AutoSize = true
            };
            this.Controls.Add(lblRelHeader);

            var relHintText = _adapter != null
                ? "Select a table above to see its potential relationships. Check to include in the model; " +
                  "double-click to toggle Active/Inactive. When no table is selected, only included relationships are shown."
                : "Define many-to-one relationships between selected tables and existing model tables. " +
                  "Enter the lookup attribute logical name (e.g. _accountid_value).";
            lblRelHint = new WinLabel
            {
                Text = relHintText,
                Location = new Point(10, 332),
                Size = new Size(840, 30),
                ForeColor = Color.DimGray
            };
            this.Controls.Add(lblRelHint);

            listViewRelationships = new ListView
            {
                Location = new Point(10, 370),
                Size = new Size(840, 200),
                View = View.Details,
                CheckBoxes = true,
                FullRowSelect = true,
                GridLines = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
            };
            listViewRelationships.Columns.Add("Source Table (many side)", 230);
            listViewRelationships.Columns.Add("Lookup Attribute", 200);
            listViewRelationships.Columns.Add("Target Table (one side)", 230);
            listViewRelationships.Columns.Add("Active", 60);
            listViewRelationships.DoubleClick += ListViewRelationships_DoubleClick;
            this.Controls.Add(listViewRelationships);

            btnAddRelationship = new Button
            {
                Text = "Add Relationship...",
                Location = new Point(10, 580),
                Size = new Size(148, 28)
            };
            btnAddRelationship.Click += BtnAddRelationship_Click;
            this.Controls.Add(btnAddRelationship);

            btnRemoveRelationship = new Button
            {
                Text = "Remove Selected",
                Location = new Point(165, 580),
                Size = new Size(130, 28)
            };
            btnRemoveRelationship.Click += BtnRemoveRelationship_Click;
            this.Controls.Add(btnRemoveRelationship);

            // ── Footer ─────────────────────────────────────────────────────────
            btnOK = new Button
            {
                Text = "OK",
                Location = new Point(490, 580),
                Size = new Size(70, 28)
            };
            btnOK.Click += BtnOK_Click;
            this.Controls.Add(btnOK);

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(570, 580),
                Size = new Size(70, 28),
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
            _checkedAdditionalTableNames.Clear();

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

                if (preChecked.Contains(table.LogicalName))
                    _checkedAdditionalTableNames.Add(table.LogicalName);

                listViewTables.Items.Add(item);
                _allTableItems.Add(item);
            }

            listViewTables.EndUpdate();
            _suppressItemCheckedEvent = false;
        }

        private void FilterTableList()
        {
            var search = txtTableSearch.Text.Trim().ToLowerInvariant();
            // "Selected only" is bypassed when the search box is in use so searching
            // always shows the full (filtered) set.
            var selectedOnly = _chkSelectedOnly.Checked && string.IsNullOrEmpty(search);

            listViewTables.BeginUpdate();
            listViewTables.Items.Clear();

            foreach (var item in _allTableItems)
            {
                if (selectedOnly)
                {
                    if (item.Tag is TableInfo tbl && _checkedAdditionalTableNames.Contains(tbl.LogicalName))
                        listViewTables.Items.Add(item);
                    continue;
                }

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

        private void BtnPasteNames_Click(object sender, EventArgs e)
        {
            using (var dlg = new PasteTableNamesDialog())
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                var names = dlg.ParsedNames;
                if (!names.Any()) return;

                var notFound = new List<string>();
                var newlyChecked = new List<TableInfo>();

                _suppressItemCheckedEvent = true;
                try
                {
                    foreach (var name in names)
                    {
                        var item = _allTableItems.FirstOrDefault(i =>
                            i.Tag is TableInfo t &&
                            (string.Equals(t.DisplayName, name, StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(t.LogicalName, name, StringComparison.OrdinalIgnoreCase)));

                        if (item?.Tag is TableInfo table)
                        {
                            if (!_checkedAdditionalTableNames.Contains(table.LogicalName))
                            {
                                _checkedAdditionalTableNames.Add(table.LogicalName);
                                item.Checked = true;
                                newlyChecked.Add(table);
                            }
                        }
                        else
                        {
                            notFound.Add(name);
                        }
                    }
                }
                finally
                {
                    _suppressItemCheckedEvent = false;
                }

                foreach (var table in newlyChecked)
                    DiscoverRelationshipsForTable(table);

                FilterTableList();
                UpdateRelationshipFilter();

                if (notFound.Any())
                {
                    MessageBox.Show(
                        $"The following names were not recognised and could not be selected:\n\n" +
                        string.Join("\n", notFound.Select(n => "  " + n)),
                        "Unrecognised Table Names",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ListViewTables_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (_suppressItemCheckedEvent) return;
            var table = e.Item.Tag as TableInfo;
            if (table == null) return;

            if (e.Item.Checked)
            {
                _checkedAdditionalTableNames.Add(table.LogicalName);
                DiscoverRelationshipsForTable(table);
            }
            else
            {
                _checkedAdditionalTableNames.Remove(table.LogicalName);
                RemoveDiscoveredRelationshipsForTable(table.LogicalName);
            }

            UpdateRelationshipFilter();
        }

        #endregion

        #region Relationship-list population

        private void PopulateRelationshipList(List<ExportRelationship> currentRelationships)
        {
            _allRelationshipItems.Clear();
            listViewRelationships.Items.Clear();

            // Pre-existing saved relationships are tagged DiscoveredForTable=null so they survive table unchecks.
            foreach (var rel in currentRelationships)
                AddRelationshipRow(rel, discoveredForTable: null, isChecked: true);
        }

        /// <summary>
        /// Adds a relationship row to the backing store. <see cref="UpdateRelationshipFilter"/> must be
        /// called afterward to update the visible ListView.
        /// </summary>
        private void AddRelationshipRow(ExportRelationship rel, string? discoveredForTable, bool isChecked)
        {
            var sourceDisplay = ResolveDisplayName(rel.SourceTable);
            var targetDisplay = ResolveDisplayName(rel.TargetTable);

            var item = new ListViewItem(sourceDisplay);
            item.SubItems.Add(rel.SourceAttribute);
            item.SubItems.Add(targetDisplay);
            item.SubItems.Add(rel.IsActive ? "Yes" : "No");
            item.Checked = isChecked;
            item.Tag = new RelationshipRowTag { Relationship = rel, DiscoveredForTable = discoveredForTable };
            _allRelationshipItems.Add(item);
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
                .Where(i => i.Tag is TableInfo t && _checkedAdditionalTableNames.Contains(t.LogicalName))
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
                {
                    AddRelationshipRow(dlg.Result, discoveredForTable: null, isChecked: true);
                    UpdateRelationshipFilter();
                }
            }
        }

        private void ListViewRelationships_DoubleClick(object sender, EventArgs e)
        {
            if (listViewRelationships.SelectedItems.Count == 0) return;
            var item = listViewRelationships.SelectedItems[0];
            if (item.Tag is RelationshipRowTag tag)
            {
                tag.Relationship.IsActive = !tag.Relationship.IsActive;
                if (item.SubItems.Count >= 4)
                    item.SubItems[3].Text = tag.Relationship.IsActive ? "Yes" : "No";
            }
        }

        private void BtnRemoveRelationship_Click(object sender, EventArgs e)
        {
            if (listViewRelationships.SelectedItems.Count == 0) return;

            foreach (ListViewItem item in listViewRelationships.SelectedItems.Cast<ListViewItem>().ToList())
            {
                listViewRelationships.Items.Remove(item);
                _allRelationshipItems.Remove(item);
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            // Collect checked tables
            SelectedAdditionalTables = _allTableItems
                .Where(i => i.Tag is TableInfo t && _checkedAdditionalTableNames.Contains(t.LogicalName))
                .Select(i => (TableInfo)i.Tag)
                .ToList();

            // Collect checked relationships from the full backing store (not just what's visible)
            var checkedRels = _allRelationshipItems
                .Where(i => i.Checked && i.Tag is RelationshipRowTag)
                .Select(i => ((RelationshipRowTag)i.Tag).Relationship)
                .ToList();

            // Validate – orphaned relationships reference tables not in the combined selection
            var allNames = new HashSet<string>(
                _alreadySelectedTableNames.Concat(SelectedAdditionalTables.Select(t => t.LogicalName)),
                StringComparer.OrdinalIgnoreCase);

            var orphaned = checkedRels
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

                checkedRels = checkedRels.Except(orphaned).ToList();
            }

            SelectedAdditionalRelationships = checkedRels;
            DialogResult = DialogResult.OK;
            Close();
        }

        #endregion

        #region Relationship discovery and filtering

        /// <summary>
        /// Returns the combined set of star-schema tables plus all currently-checked additional tables.
        /// This is the full context within which we look for potential relationships.
        /// </summary>
        private HashSet<string> GetAllSelectedTableNames()
        {
            var all = new HashSet<string>(_alreadySelectedTableNames, StringComparer.OrdinalIgnoreCase);
            all.UnionWith(_checkedAdditionalTableNames);
            return all;
        }

        /// <summary>
        /// Queries Dataverse for relationships between <paramref name="table"/> and all currently-selected
        /// tables (star-schema + other checked additionals). M:1 rows are checked by default; 1:M rows are
        /// unchecked so the user can opt in. Duplicate relationships are skipped.
        /// </summary>
        private void DiscoverRelationshipsForTable(TableInfo table)
        {
            if (_adapter == null || _service == null) return;

            var allSelected = GetAllSelectedTableNames();
            allSelected.Remove(table.LogicalName); // don’t relate a table to itself

            var prev = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                // ─ M:1 — table has a lookup pointing TO another selected table ────────────────────
                if (!_attributeCache.TryGetValue(table.LogicalName, out var attrs))
                {
                    attrs = _adapter.GetAttributesSync(_service, table.LogicalName);
                    _attributeCache[table.LogicalName] = attrs;
                }

                foreach (var lookup in attrs.Where(a =>
                    a.AttributeType == "Lookup" && a.Targets != null && a.Targets.Any()))
                {
                    foreach (var target in lookup.Targets.Where(t => allSelected.Contains(t)))
                    {
                        var rel = new ExportRelationship
                        {
                            SourceTable = table.LogicalName,
                            SourceAttribute = lookup.LogicalName,
                            TargetTable = target,
                            DisplayName = lookup.DisplayName ?? lookup.LogicalName,
                            IsActive = true,
                            AssumeReferentialIntegrity = lookup.IsRequired
                        };
                        if (!IsDuplicateRelationship(rel))
                            AddRelationshipRow(rel, discoveredForTable: table.LogicalName, isChecked: true);
                    }
                }

                // ─ 1:M — another selected table has a lookup pointing TO this table ─────────────
                if (!_oneToManyCache.TryGetValue(table.LogicalName, out var o2mRels))
                {
                    o2mRels = _adapter.GetOneToManyRelationshipsSync(_service, table.LogicalName);
                    _oneToManyCache[table.LogicalName] = o2mRels;
                }

                foreach (var o2m in o2mRels.Where(r => allSelected.Contains(r.ReferencingEntity)))
                {
                    var rel = new ExportRelationship
                    {
                        SourceTable = o2m.ReferencingEntity,
                        SourceAttribute = o2m.ReferencingAttribute,
                        TargetTable = table.LogicalName,
                        DisplayName = o2m.LookupDisplayName ?? o2m.ReferencingAttribute,
                        IsActive = true
                    };
                    if (!IsDuplicateRelationship(rel))
                        AddRelationshipRow(rel, discoveredForTable: table.LogicalName, isChecked: false);
                }
            }
            finally
            {
                this.Cursor = prev;
            }
        }

        /// <summary>
        /// Removes all relationship rows that were discovered in the context of <paramref name="logicalName"/>.
        /// Rows with <c>DiscoveredForTable = null</c> (manually-added or pre-existing) are preserved.
        /// </summary>
        private void RemoveDiscoveredRelationshipsForTable(string logicalName)
        {
            _allRelationshipItems.RemoveAll(item =>
                item.Tag is RelationshipRowTag tag &&
                string.Equals(tag.DiscoveredForTable, logicalName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Repopulates the visible relationship ListView based on the currently-highlighted table.
        /// When a table is selected, shows all relationships involving that table (checked + unchecked).
        /// When nothing is selected, shows only the included (checked) relationships.
        /// </summary>
        private void UpdateRelationshipFilter()
        {
            var selectedTable = listViewTables.SelectedItems.Count > 0
                ? listViewTables.SelectedItems[0].Tag as TableInfo
                : null;

            listViewRelationships.BeginUpdate();
            listViewRelationships.Items.Clear();

            foreach (var item in _allRelationshipItems)
            {
                bool show;
                if (selectedTable != null)
                {
                    if (item.Tag is RelationshipRowTag tag)
                    {
                        var r = tag.Relationship;
                        show = r.SourceTable.Equals(selectedTable.LogicalName, StringComparison.OrdinalIgnoreCase)
                            || r.TargetTable.Equals(selectedTable.LogicalName, StringComparison.OrdinalIgnoreCase);
                    }
                    else show = false;
                }
                else
                {
                    show = item.Checked;
                }

                if (show)
                    listViewRelationships.Items.Add(item);
            }

            listViewRelationships.EndUpdate();
        }

        /// <summary>Returns true if an identical SourceTable/SourceAttribute/TargetTable row already exists.</summary>
        private bool IsDuplicateRelationship(ExportRelationship rel)
        {
            return _allRelationshipItems.Any(item =>
            {
                if (item.Tag is RelationshipRowTag tag)
                {
                    var r = tag.Relationship;
                    return string.Equals(r.SourceTable, rel.SourceTable, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(r.SourceAttribute, rel.SourceAttribute, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(r.TargetTable, rel.TargetTable, StringComparison.OrdinalIgnoreCase);
                }
                return false;
            });
        }

        /// <summary>
        /// On dialog re-open: runs discovery for all pre-checked tables. Leverages caches and the
        /// duplicate check so rows already loaded from <c>currentAdditionalRelationships</c> are not doubled.
        /// </summary>
        private void LoadDiscoveredRelationshipsForPreCheckedTables()
        {
            if (_adapter == null || _service == null) return;

            var preChecked = _allTableItems
                .Where(i => i.Tag is TableInfo t && _checkedAdditionalTableNames.Contains(t.LogicalName))
                .Select(i => (TableInfo)i.Tag)
                .ToList();

            foreach (var table in preChecked)
                DiscoverRelationshipsForTable(table);
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

        #region Nested: RelationshipRowTag

        /// <summary>Tag stored on each relationship <see cref="ListViewItem"/>.</summary>
        private class RelationshipRowTag
        {
            /// <summary>The relationship this row represents.</summary>
            public ExportRelationship Relationship { get; set; } = null!;

            /// <summary>
            /// Logical name of the additional table whose check triggered this row's discovery,
            /// or <c>null</c> for manually-added or pre-existing relationships.
            /// </summary>
            public string? DiscoveredForTable { get; set; }
        }

        #endregion

        #region Nested: PasteTableNamesDialog

        /// <summary>
        /// Minimal dialog for pasting a list of display names or logical names to bulk-select tables.
        /// </summary>
        private class PasteTableNamesDialog : Form
        {
            private TextBox txtNames = null!;
            private Button btnSelect = null!;
            private Button btnCancel = null!;

            /// <summary>The parsed names entered by the user (display names or logical names).</summary>
            public List<string> ParsedNames { get; private set; } = new List<string>();

            public PasteTableNamesDialog()
            {
                InitializeComponent();
            }

            private void InitializeComponent()
            {
                this.Text = "Quick Select Tables";
                this.ClientSize = new Size(480, 350);
                this.StartPosition = FormStartPosition.CenterParent;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;
                this.MinimizeBox = false;

                var lbl = new WinLabel
                {
                    Text = "Paste display names or logical names below, separated by commas or one per line.\n" +
                           "Matching tables will be selected. Existing selections are not cleared.",
                    Location = new Point(15, 15),
                    Size = new Size(445, 44),
                    AutoSize = false
                };
                this.Controls.Add(lbl);

                txtNames = new TextBox
                {
                    Location = new Point(15, 68),
                    Size = new Size(445, 218),
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    AcceptsReturn = true,
                    Font = new Font("Consolas", 9f)
                };
                this.Controls.Add(txtNames);

                btnSelect = new Button
                {
                    Text = "Select",
                    Location = new Point(290, 296),
                    Size = new Size(75, 28),
                    DialogResult = DialogResult.OK
                };
                btnSelect.Click += (s, ev) =>
                {
                    ParsedNames = txtNames.Text
                        .Split(new[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(n => n.Trim())
                        .Where(n => n.Length > 0)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                };
                this.Controls.Add(btnSelect);

                btnCancel = new Button
                {
                    Text = "Cancel",
                    Location = new Point(375, 296),
                    Size = new Size(75, 28),
                    DialogResult = DialogResult.Cancel
                };
                this.Controls.Add(btnCancel);

                this.AcceptButton = btnSelect;
                this.CancelButton = btnCancel;
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

                this.Controls.Add(new WinLabel { Text = "Source Table (many side):", Location = new Point(10, y + 4), Width = labelW });
                cmbSourceTable = new ComboBox
                {
                    Location = new Point(comboX, y),
                    Width = comboW,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                this.Controls.Add(cmbSourceTable);
                y += rowH;

                this.Controls.Add(new WinLabel
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
                this.Controls.Add(new WinLabel
                {
                    Text = "e.g. _accountid_value",
                    Location = new Point(comboX, y + 24),
                    Width = comboW,
                    ForeColor = Color.Gray
                });
                y += rowH + 16;

                this.Controls.Add(new WinLabel { Text = "Target Table (one side):", Location = new Point(10, y + 4), Width = labelW });
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
