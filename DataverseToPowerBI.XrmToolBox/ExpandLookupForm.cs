// ===================================================================================
// ExpandLookupForm.cs - Expand Lookup Field to Include Related Table Attributes
// ===================================================================================
//
// PURPOSE:
// This dialog allows users to select attributes from a related table (via a lookup
// field) to flatten into the parent table. Instead of creating a separate dimension
// table and relationship, the selected columns are brought in via a LEFT OUTER JOIN
// in the generated SQL.
//
// USE CASES:
// - Pull a few reference fields (e.g., Account Name, Industry) without a full dim
// - Flatten lookup references for simpler, denormalized reporting
// - Reduce the number of tables in the semantic model
//
// UI FLOW:
// 1. User sees the related table name and lookup field info at top
// 2. User selects a form to filter available attributes (single-target lookups only)
// 3. User checks attributes to include from the related table(s)
// 4. Performance warnings shown if thresholds exceeded
//
// Expand Lookup: always enabled for Lookup/Owner/Customer attribute types
//
// ===================================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DataverseToPowerBI.Core.Models;

using CoreAttributeMetadata = DataverseToPowerBI.Core.Models.AttributeMetadata;
using WinLabel = System.Windows.Forms.Label;

namespace DataverseToPowerBI.XrmToolBox
{
    /// <summary>
    /// Target-table metadata used by the expand-lookup dialog.
    /// </summary>
    public sealed class ExpandLookupTargetContext
    {
        public string TableLogicalName { get; set; } = "";
        public string TableDisplayName { get; set; } = "";
        public string TablePrimaryKey { get; set; } = "";
        public int ObjectTypeCode { get; set; }
        public List<FormMetadata> Forms { get; set; } = new List<FormMetadata>();
        public List<CoreAttributeMetadata> Attributes { get; set; } = new List<CoreAttributeMetadata>();
    }

    /// <summary>
    /// Dialog for selecting attributes from one or more related tables to flatten into the parent table.
    /// </summary>
    public class ExpandLookupForm : Form
    {
        private const int MAX_RECOMMENDED_FIELDS = 10;
        private const int MAX_RECOMMENDED_EXPANDS = 3;
        private const int CONTENT_LEFT = 15;
        private const int CONTENT_WIDTH = 550;
        private const int BUTTON_ROW_HEIGHT = 28;

        private readonly string _lookupAttributeName;
        private readonly string _lookupDisplayName;
        private readonly List<ExpandLookupTargetContext> _targetContexts;
        private readonly List<ExpandedLookupAttribute>? _existingSelection;
        private readonly string? _existingFormId;
        private readonly bool _existingIncludeRelatedRecordLink;
        private readonly int _currentExpandCount;
        private readonly HashSet<string> _selectedAttributeKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _legacySelectedAttributeLogicalNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private WinLabel lblLookupInfo = null!;
        private WinLabel lblTargetTableCaption = null!;
        private FlowLayoutPanel pnlTargetTables = null!;
        private WinLabel lblForm = null!;
        private ComboBox cboForm = null!;
        private WinLabel lblSearch = null!;
        private TextBox txtSearch = null!;
        private WinLabel lblFormFieldCount = null!;
        private WinLabel lblWarning = null!;
        private Panel pnlWarning = null!;
        private CheckBox chkIncludeRelatedRecordLink = null!;
        private ListView listViewAttributes = null!;
        private Button btnOk = null!;
        private Button btnCancel = null!;
        private Button btnDeselectAll = null!;
        private WinLabel lblStatus = null!;

        private bool _isLoading;
        private int _sortColumn = 1;
        private bool _sortAscending = true;

        /// <summary>
        /// The selected attributes from the related table(s).
        /// </summary>
        public List<ExpandedLookupAttribute> SelectedAttributes { get; private set; } = new List<ExpandedLookupAttribute>();

        /// <summary>
        /// The form ID selected by the user for single-target lookups.
        /// </summary>
        public string? SelectedFormId { get; private set; }

        /// <summary>
        /// Whether to generate a related-record link column for this lookup.
        /// </summary>
        public bool IncludeRelatedRecordLink => chkIncludeRelatedRecordLink.Checked;

        private bool IsPolymorphic => _targetContexts.Count > 1;

        public ExpandLookupForm(
            string lookupAttributeName,
            string lookupDisplayName,
            List<ExpandLookupTargetContext> targetContexts,
            List<ExpandedLookupAttribute>? existingSelection = null,
            string? existingFormId = null,
            bool existingIncludeRelatedRecordLink = false,
            int currentExpandCount = 0)
        {
            _lookupAttributeName = lookupAttributeName;
            _lookupDisplayName = lookupDisplayName;
            _targetContexts = targetContexts ?? throw new ArgumentNullException(nameof(targetContexts));
            _existingSelection = existingSelection;
            _existingFormId = existingFormId;
            _existingIncludeRelatedRecordLink = existingIncludeRelatedRecordLink;
            _currentExpandCount = currentExpandCount;

            if (_existingSelection != null)
            {
                foreach (var attribute in _existingSelection)
                {
                    if (!string.IsNullOrWhiteSpace(attribute.TargetTableLogicalName))
                        _selectedAttributeKeys.Add(BuildSelectionKey(attribute.TargetTableLogicalName, attribute.LogicalName));
                    else
                        _legacySelectedAttributeLogicalNames.Add(attribute.LogicalName);
                }
            }

            InitializeComponent();
            PopulateFormDropdown();
        }

        private void InitializeComponent()
        {
            Text = $"Expand Lookup - {_lookupDisplayName}";
            Size = new Size(600, 625);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var y = 15;

            lblLookupInfo = new WinLabel
            {
                Text = $"Lookup Field: {_lookupDisplayName} ({_lookupAttributeName})",
                Location = new Point(CONTENT_LEFT, y),
                AutoSize = true,
                Font = new Font(Font, FontStyle.Bold)
            };
            Controls.Add(lblLookupInfo);
            y += 25;

            lblTargetTableCaption = new WinLabel
            {
                Text = IsPolymorphic ? "Related Tables:" : "Related Table:",
                Location = new Point(CONTENT_LEFT, y),
                AutoSize = true
            };
            Controls.Add(lblTargetTableCaption);

            pnlTargetTables = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            Controls.Add(pnlTargetTables);
            PopulateTargetTableLinks();
            y += 38;

            chkIncludeRelatedRecordLink = new CheckBox
            {
                Text = "Include link to the related record using this lookup value",
                Location = new Point(CONTENT_LEFT, y),
                AutoSize = true,
                Checked = _existingIncludeRelatedRecordLink
            };
            chkIncludeRelatedRecordLink.CheckedChanged += ChkIncludeRelatedRecordLink_CheckedChanged;
            Controls.Add(chkIncludeRelatedRecordLink);
            y += 24;

            pnlWarning = new Panel
            {
                Location = new Point(CONTENT_LEFT, y),
                Size = new Size(CONTENT_WIDTH, 50),
                BackColor = Color.FromArgb(255, 248, 220),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            lblWarning = new WinLabel
            {
                Text = "",
                Location = new Point(8, 5),
                Size = new Size(530, 40),
                ForeColor = Color.DarkGoldenrod
            };
            pnlWarning.Controls.Add(lblWarning);
            Controls.Add(pnlWarning);

            lblForm = new WinLabel
            {
                Text = IsPolymorphic ? "Scope:" : "Form:",
                Location = new Point(CONTENT_LEFT, y + 3),
                AutoSize = true
            };
            Controls.Add(lblForm);

            cboForm = new ComboBox
            {
                Location = new Point(60, y),
                Size = new Size(380, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = !IsPolymorphic
            };
            cboForm.SelectedIndexChanged += CboForm_SelectedIndexChanged;
            Controls.Add(cboForm);

            y += 30;

            lblSearch = new WinLabel
            {
                Text = "Search:",
                Location = new Point(CONTENT_LEFT, y + 3),
                AutoSize = true
            };
            Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Location = new Point(60, y),
                Size = new Size(380, 23)
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            Controls.Add(txtSearch);

            lblFormFieldCount = new WinLabel
            {
                Text = "",
                Location = new Point(450, y + 3),
                AutoSize = true,
                ForeColor = Color.Gray
            };
            Controls.Add(lblFormFieldCount);
            y += 35;

            listViewAttributes = new ListView
            {
                Location = new Point(CONTENT_LEFT, y),
                Size = new Size(CONTENT_WIDTH, 320),
                View = View.Details,
                FullRowSelect = true,
                CheckBoxes = true,
                ShowGroups = IsPolymorphic
            };
            listViewAttributes.Columns.Add("Sel", 35);
            listViewAttributes.Columns.Add("Display Name", 180);
            listViewAttributes.Columns.Add("Related Table", 120);
            listViewAttributes.Columns.Add("Logical Name", 135);
            listViewAttributes.Columns.Add("Type", 80);
            listViewAttributes.ColumnClick += ListViewAttributes_ColumnClick;
            listViewAttributes.ItemChecked += ListViewAttributes_ItemChecked;
            Controls.Add(listViewAttributes);
            y += 325;

            btnDeselectAll = new Button
            {
                Text = "Deselect All",
                Location = new Point(15, y + 5),
                Size = new Size(85, 28)
            };
            btnDeselectAll.Click += BtnDeselectAll_Click;
            Controls.Add(btnDeselectAll);

            lblStatus = new WinLabel
            {
                Text = "Select a form to see available attributes.",
                Location = new Point(110, y + 10),
                AutoSize = false,
                Size = new Size(280, 28),
                AutoEllipsis = true,
                ForeColor = Color.Gray
            };
            Controls.Add(lblStatus);

            btnOk = new Button
            {
                Text = "OK",
                Location = new Point(400, y + 5),
                Size = new Size(75, 28),
                DialogResult = DialogResult.OK,
                Enabled = false
            };
            btnOk.Click += BtnOk_Click;
            Controls.Add(btnOk);

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(485, y + 5),
                Size = new Size(75, 28),
                DialogResult = DialogResult.Cancel
            };
            Controls.Add(btnCancel);

            AcceptButton = btnOk;
            CancelButton = btnCancel;

            LayoutControls();
        }

        private void LayoutControls()
        {
            var y = 15;

            lblLookupInfo.Location = new Point(CONTENT_LEFT, y);
            y += 25;

            lblTargetTableCaption.Location = new Point(CONTENT_LEFT, y + 3);
            var targetPanelLeft = lblTargetTableCaption.Right + 5;
            var targetPanelWidth = Math.Max(120, CONTENT_LEFT + CONTENT_WIDTH - targetPanelLeft);
            pnlTargetTables.MaximumSize = new Size(targetPanelWidth, 0);
            var targetPanelHeight = Math.Max(23, pnlTargetTables.GetPreferredSize(new Size(targetPanelWidth, 0)).Height);
            pnlTargetTables.Location = new Point(targetPanelLeft, y);
            pnlTargetTables.Size = new Size(targetPanelWidth, targetPanelHeight);
            y += targetPanelHeight + 10;

            chkIncludeRelatedRecordLink.Location = new Point(CONTENT_LEFT, y);
            y += 24;

            pnlWarning.Location = new Point(CONTENT_LEFT, y);
            if (pnlWarning.Visible)
                y += pnlWarning.Height + 5;

            lblForm.Location = new Point(CONTENT_LEFT, y + 3);
            cboForm.Location = new Point(60, y);
            y += 35;

            lblSearch.Location = new Point(CONTENT_LEFT, y + 3);
            txtSearch.Location = new Point(60, y);
            lblFormFieldCount.Location = new Point(450, y + 3);
            y += 35;

            var buttonTop = ClientSize.Height - BUTTON_ROW_HEIGHT - 15;
            var listHeight = Math.Max(220, buttonTop - y - 10);

            listViewAttributes.Location = new Point(CONTENT_LEFT, y);
            listViewAttributes.Size = new Size(CONTENT_WIDTH, listHeight);

            btnDeselectAll.Location = new Point(CONTENT_LEFT, buttonTop);
            lblStatus.Location = new Point(110, buttonTop + 5);
            btnOk.Location = new Point(400, buttonTop);
            btnCancel.Location = new Point(485, buttonTop);
            lblStatus.Size = new Size(Math.Max(80, btnOk.Left - lblStatus.Left - 10), BUTTON_ROW_HEIGHT);
        }

        private void PopulateTargetTableLinks()
        {
            pnlTargetTables.SuspendLayout();
            pnlTargetTables.Controls.Clear();

            var orderedTargets = _targetContexts
                .OrderBy(target => target.TableDisplayName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            for (var i = 0; i < orderedTargets.Count; i++)
            {
                var target = orderedTargets[i];
                var link = new LinkLabel
                {
                    AutoSize = true,
                    Margin = Padding.Empty,
                    Padding = Padding.Empty,
                    Text = IsPolymorphic
                        ? target.TableDisplayName
                        : $"{target.TableDisplayName} ({target.TableLogicalName})",
                    Tag = target.TableLogicalName
                };
                link.LinkClicked += TargetTableLink_LinkClicked;
                pnlTargetTables.Controls.Add(link);

                if (i < orderedTargets.Count - 1)
                {
                    pnlTargetTables.Controls.Add(new WinLabel
                    {
                        AutoSize = true,
                        Margin = Padding.Empty,
                        Padding = Padding.Empty,
                        Text = ", "
                    });
                }
            }

            pnlTargetTables.ResumeLayout();
        }

        private void PopulateFormDropdown()
        {
            cboForm.Items.Clear();

            if (IsPolymorphic)
            {
                cboForm.Items.Add("(All related tables)");
                cboForm.SelectedIndex = 0;
                PopulateAttributes();
                return;
            }

            var target = _targetContexts[0];
            cboForm.Items.Add("(All Attributes)");

            foreach (var form in target.Forms.OrderBy(f => f.Name))
            {
                var fieldCount = form.Fields?.Count ?? 0;
                cboForm.Items.Add(new FormComboItem(form, fieldCount));
            }

            if (!string.IsNullOrEmpty(_existingFormId))
            {
                for (var i = 1; i < cboForm.Items.Count; i++)
                {
                    if (cboForm.Items[i] is FormComboItem item && item.Form.FormId == _existingFormId)
                    {
                        cboForm.SelectedIndex = i;
                        return;
                    }
                }
            }

            cboForm.SelectedIndex = cboForm.Items.Count > 1 ? 1 : 0;
        }

        private void CboForm_SelectedIndexChanged(object? sender, EventArgs e)
        {
            PopulateAttributes();
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            PopulateAttributes();
        }

        private void ListViewAttributes_ColumnClick(object? sender, ColumnClickEventArgs e)
        {
            if (e.Column == _sortColumn)
                _sortAscending = !_sortAscending;
            else
            {
                _sortColumn = e.Column;
                _sortAscending = true;
            }

            ApplyListSort();
        }

        private void ChkIncludeRelatedRecordLink_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateStatus();
            UpdateWarnings();
        }

        private void PopulateAttributes()
        {
            _isLoading = true;
            listViewAttributes.BeginUpdate();
            listViewAttributes.Items.Clear();
            listViewAttributes.Groups.Clear();

            HashSet<string>? formFields = null;
            if (!IsPolymorphic && cboForm.SelectedItem is FormComboItem formItem)
            {
                formFields = formItem.Form.Fields != null
                    ? new HashSet<string>(formItem.Form.Fields, StringComparer.OrdinalIgnoreCase)
                    : null;
                lblFormFieldCount.Text = formFields != null ? $"{formFields.Count} fields" : "";
                SelectedFormId = formItem.Form.FormId;
            }
            else
            {
                var totalAttributes = _targetContexts.Sum(t => t.Attributes.Count);
                lblFormFieldCount.Text = IsPolymorphic ? $"{totalAttributes} total" : $"{_targetContexts[0].Attributes.Count} total";
                SelectedFormId = null;
            }

            var searchText = txtSearch.Text?.Trim() ?? string.Empty;

            var attributeOptions = _targetContexts
                .SelectMany(target => target.Attributes.Select(attr => new AttributeOption(target, attr)))
                .Where(option => !IsExcludedAttribute(option.Attribute, option.Target.TablePrimaryKey))
                .Where(option => HasDisplayName(option.Attribute))
                .Where(option => IsPolymorphic ||
                    formFields == null ||
                    formFields.Contains(option.Attribute.LogicalName) ||
                    _selectedAttributeKeys.Contains(BuildSelectionKey(option.Target.TableLogicalName, option.Attribute.LogicalName)) ||
                    _legacySelectedAttributeLogicalNames.Contains(option.Attribute.LogicalName))
                .Where(option => MatchesSearch(option, searchText))
                .OrderBy(option => option.Attribute.DisplayName ?? option.Attribute.LogicalName)
                .ThenBy(option => option.Target.TableDisplayName)
                .ToList();

            foreach (var option in attributeOptions)
            {
                var selectionKey = BuildSelectionKey(option.Target.TableLogicalName, option.Attribute.LogicalName);
                var isSelected = _selectedAttributeKeys.Contains(selectionKey) ||
                    _legacySelectedAttributeLogicalNames.Contains(option.Attribute.LogicalName);

                var item = new ListViewItem("")
                {
                    Checked = isSelected,
                    Tag = option,
                    Name = selectionKey
                };
                item.SubItems.Add(option.Attribute.DisplayName ?? option.Attribute.LogicalName);
                item.SubItems.Add(option.Target.TableDisplayName);
                item.SubItems.Add(option.Attribute.LogicalName);
                item.SubItems.Add(option.Attribute.AttributeType ?? "");

                if (formFields != null && !formFields.Contains(option.Attribute.LogicalName) && !isSelected)
                    item.ForeColor = Color.LightGray;

                listViewAttributes.Items.Add(item);
            }

            if (IsPolymorphic)
            {
                ApplyPolymorphicGrouping();
            }
            else
            {
                ApplyListSort();
            }

            listViewAttributes.EndUpdate();
            _isLoading = false;
            UpdateStatus();
            UpdateWarnings();
        }

        private void ApplyPolymorphicGrouping()
        {
            listViewAttributes.ShowGroups = true;

            var groupedItems = listViewAttributes.Items.Cast<ListViewItem>()
                .Where(item => item.Tag is AttributeOption)
                .GroupBy(item => ((AttributeOption)item.Tag).Target.TableLogicalName, StringComparer.OrdinalIgnoreCase)
                .OrderBy(group => ((AttributeOption)group.First().Tag).Target.TableDisplayName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            var groupsByTarget = new Dictionary<string, ListViewGroup>(StringComparer.OrdinalIgnoreCase);

            foreach (var group in groupedItems)
            {
                var firstOption = (AttributeOption)group.First().Tag;
                var listGroup = new ListViewGroup(
                    firstOption.Target.TableDisplayName,
                    firstOption.Target.TableDisplayName)
                {
                    HeaderAlignment = HorizontalAlignment.Left
                };

                listViewAttributes.Groups.Add(listGroup);
                groupsByTarget[group.Key] = listGroup;
            }

            foreach (ListViewItem item in listViewAttributes.Items)
            {
                if (item.Tag is not AttributeOption option)
                    continue;

                if (groupsByTarget.TryGetValue(option.Target.TableLogicalName, out var group))
                    item.Group = group;
            }
        }

        private void ApplyListSort()
        {
            if (listViewAttributes.Items.Count == 0)
                return;

            listViewAttributes.ListViewItemSorter = new AttributeListViewItemComparer(_sortColumn, _sortAscending);
            listViewAttributes.Sort();
        }

        private static bool IsExcludedAttribute(CoreAttributeMetadata attr, string targetTablePrimaryKey)
        {
            if (attr.AttributeType?.Equals("Virtual", StringComparison.OrdinalIgnoreCase) == true)
                return true;

            if (attr.LogicalName.Equals(targetTablePrimaryKey, StringComparison.OrdinalIgnoreCase))
                return true;

            if (attr.LogicalName.Equals("statecode", StringComparison.OrdinalIgnoreCase) ||
                attr.LogicalName.Equals("statuscode", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        private static bool HasDisplayName(CoreAttributeMetadata attr)
        {
            if (string.IsNullOrWhiteSpace(attr.DisplayName))
                return false;

            return !string.Equals(
                attr.DisplayName.Trim(),
                attr.LogicalName?.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }

        private void ListViewAttributes_ItemChecked(object? sender, ItemCheckedEventArgs e)
        {
            if (_isLoading) return;

            if (e.Item.Tag is AttributeOption option)
            {
                var selectionKey = BuildSelectionKey(option.Target.TableLogicalName, option.Attribute.LogicalName);
                _legacySelectedAttributeLogicalNames.Remove(option.Attribute.LogicalName);
                if (e.Item.Checked)
                    _selectedAttributeKeys.Add(selectionKey);
                else
                    _selectedAttributeKeys.Remove(selectionKey);
            }

            UpdateStatus();
            UpdateWarnings();
        }

        private static bool MatchesSearch(AttributeOption option, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return true;

            return (option.Attribute.DisplayName?.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) ?? -1) >= 0 ||
                option.Attribute.LogicalName.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private void TargetTableLink_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            if (sender is not LinkLabel link || link.Tag is not string targetTableLogicalName)
                return;

            var targetItem = listViewAttributes.Items.Cast<ListViewItem>()
                .FirstOrDefault(item =>
                    item.Tag is AttributeOption option &&
                    string.Equals(option.Target.TableLogicalName, targetTableLogicalName, StringComparison.OrdinalIgnoreCase));

            if (targetItem == null && !string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Clear();
                targetItem = listViewAttributes.Items.Cast<ListViewItem>()
                    .FirstOrDefault(item =>
                        item.Tag is AttributeOption option &&
                        string.Equals(option.Target.TableLogicalName, targetTableLogicalName, StringComparison.OrdinalIgnoreCase));
            }

            if (targetItem == null)
                return;

            targetItem.Selected = true;
            targetItem.Focused = true;
            targetItem.EnsureVisible();
        }

        private int GetCheckedAttributeCount()
        {
            return listViewAttributes.CheckedItems.Count;
        }

        private IEnumerable<AttributeOption> GetCheckedAttributeOptions()
        {
            return listViewAttributes.CheckedItems.Cast<ListViewItem>()
                .Where(item => item.Tag is AttributeOption)
                .Select(item => (AttributeOption)item.Tag);
        }

        private void UpdateStatus()
        {
            var checkedCount = GetCheckedAttributeCount();
            if (checkedCount == 0)
            {
                lblStatus.Text = chkIncludeRelatedRecordLink.Checked
                    ? "No attributes selected. Related record link will still be generated."
                    : "No attributes selected (will remove expansion).";
            }
            else
            {
                lblStatus.Text = $"{checkedCount} attribute{(checkedCount == 1 ? "" : "s")} selected" +
                    (chkIncludeRelatedRecordLink.Checked ? "; related record link included." : ".");
            }

            btnOk.Enabled = true;
        }

        private void UpdateWarnings()
        {
            var checkedCount = GetCheckedAttributeCount();
            var warnings = new List<string>();

            if (checkedCount >= MAX_RECOMMENDED_FIELDS)
            {
                warnings.Add($"⚠ {checkedCount} fields selected. Selecting {MAX_RECOMMENDED_FIELDS}+ fields from a single expanded lookup may impact DirectQuery performance.");
            }

            var totalExpands = _currentExpandCount + (checkedCount > 0 ? 1 : 0);
            if (totalExpands >= MAX_RECOMMENDED_EXPANDS)
            {
                warnings.Add($"⚠ This table has {totalExpands} expanded lookups. {MAX_RECOMMENDED_EXPANDS}+ expanded lookups on a single table may impact performance.");
            }

            if (warnings.Count > 0)
            {
                lblWarning.Text = string.Join("\r\n", warnings);
                pnlWarning.Visible = true;
                pnlWarning.Height = warnings.Count > 1 ? 50 : 35;
            }
            else
            {
                pnlWarning.Visible = false;
            }

            LayoutControls();
        }

        private void BtnDeselectAll_Click(object? sender, EventArgs e)
        {
            _isLoading = true;
            foreach (ListViewItem item in listViewAttributes.Items)
                item.Checked = false;
            _isLoading = false;
            _selectedAttributeKeys.Clear();
            _legacySelectedAttributeLogicalNames.Clear();
            UpdateStatus();
            UpdateWarnings();
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            SelectedAttributes = GetCheckedAttributeOptions()
                .Select(option => new ExpandedLookupAttribute
                {
                    LogicalName = option.Attribute.LogicalName,
                    DisplayName = option.Attribute.DisplayName,
                    TargetTableLogicalName = option.Target.TableLogicalName,
                    TargetTableDisplayName = option.Target.TableDisplayName,
                    TargetTablePrimaryKey = option.Target.TablePrimaryKey,
                    TargetTableObjectTypeCode = option.Target.ObjectTypeCode,
                    AttributeType = option.Attribute.AttributeType,
                    SchemaName = option.Attribute.SchemaName,
                    Targets = option.Attribute.Targets,
                    VirtualAttributeName = option.Attribute.VirtualAttributeName,
                    IsGlobal = option.Attribute.IsGlobal,
                    OptionSetName = option.Attribute.OptionSetName,
                    IncludeInModel = true
                })
                .ToList();
        }

        private static string BuildSelectionKey(string? tableLogicalName, string attributeLogicalName)
        {
            return string.IsNullOrWhiteSpace(tableLogicalName)
                ? attributeLogicalName
                : $"{tableLogicalName}|{attributeLogicalName}";
        }

        private sealed class AttributeOption
        {
            public AttributeOption(ExpandLookupTargetContext target, CoreAttributeMetadata attribute)
            {
                Target = target;
                Attribute = attribute;
            }

            public ExpandLookupTargetContext Target { get; }
            public CoreAttributeMetadata Attribute { get; }
        }

        private sealed class FormComboItem
        {
            public FormComboItem(FormMetadata form, int fieldCount)
            {
                Form = form;
                FieldCount = fieldCount;
            }

            public FormMetadata Form { get; }
            public int FieldCount { get; }

            public override string ToString()
            {
                return $"{Form.Name} ({FieldCount} fields)";
            }
        }

        private sealed class AttributeListViewItemComparer : IComparer
        {
            private readonly int _column;
            private readonly bool _ascending;

            public AttributeListViewItemComparer(int column, bool ascending)
            {
                _column = column;
                _ascending = ascending;
            }

            public int Compare(object? x, object? y)
            {
                var left = x as ListViewItem;
                var right = y as ListViewItem;

                if (left == null || right == null)
                    return 0;

                int result;
                if (_column == 0)
                {
                    result = left.Checked.CompareTo(right.Checked);
                }
                else
                {
                    var leftText = left.SubItems.Count > _column ? left.SubItems[_column].Text : string.Empty;
                    var rightText = right.SubItems.Count > _column ? right.SubItems[_column].Text : string.Empty;
                    result = StringComparer.CurrentCultureIgnoreCase.Compare(leftText, rightText);
                }

                return _ascending ? result : -result;
            }
        }
    }
}