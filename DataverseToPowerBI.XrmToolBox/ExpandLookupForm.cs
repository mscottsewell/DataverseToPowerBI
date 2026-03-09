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
// 2. User selects a form to filter available attributes
// 3. User checks attributes to include from that form
// 4. Performance warnings shown if thresholds exceeded
//
// Expand Lookup: always enabled for Lookup/Owner/Customer attribute types
//
// ===================================================================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Collections;
using System.Windows.Forms;
using DataverseToPowerBI.Core.Models;

using CoreAttributeMetadata = DataverseToPowerBI.Core.Models.AttributeMetadata;
using WinLabel = System.Windows.Forms.Label;

namespace DataverseToPowerBI.XrmToolBox
{
    /// <summary>
    /// Dialog for selecting attributes from a related table to expand (flatten) into the parent table.
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
        private readonly string _targetTableLogicalName;
        private readonly string _targetTableDisplayName;
        private readonly string _targetTablePrimaryKey;
        private readonly List<FormMetadata> _forms;
        private readonly List<CoreAttributeMetadata> _allAttributes;
        private readonly List<ExpandedLookupAttribute>? _existingSelection;
        private readonly string? _existingFormId;
        private readonly bool _existingIncludeRelatedRecordLink;
        private readonly int _currentExpandCount;

        // UI Controls
        private WinLabel lblLookupInfo = null!;
        private WinLabel lblTargetTable = null!;
        private WinLabel lblForm = null!;
        private ComboBox cboForm = null!;
        private WinLabel lblFormFieldCount = null!;
        private WinLabel lblWarning = null!;
        private Panel pnlWarning = null!;
        private CheckBox chkIncludeRelatedRecordLink = null!;
        private ListView listViewAttributes = null!;
        private Button btnOk = null!;
        private Button btnCancel = null!;
        private Button btnSelectAll = null!;
        private Button btnDeselectAll = null!;
        private WinLabel lblStatus = null!;

        private bool _isLoading = false;
        private int _sortColumn = 1;
        private bool _sortAscending = true;

        /// <summary>
        /// The selected attributes from the related table.
        /// </summary>
        public List<ExpandedLookupAttribute> SelectedAttributes { get; private set; } = new List<ExpandedLookupAttribute>();

        /// <summary>
        /// The form ID selected by the user.
        /// </summary>
        public string? SelectedFormId { get; private set; }

        /// <summary>
        /// Whether to generate a related-record link measure for this lookup.
        /// </summary>
        public bool IncludeRelatedRecordLink => chkIncludeRelatedRecordLink.Checked;

        public ExpandLookupForm(
            string lookupAttributeName,
            string lookupDisplayName,
            string targetTableLogicalName,
            string targetTableDisplayName,
            string targetTablePrimaryKey,
            List<FormMetadata> forms,
            List<CoreAttributeMetadata> allAttributes,
            List<ExpandedLookupAttribute>? existingSelection = null,
            string? existingFormId = null,
            bool existingIncludeRelatedRecordLink = false,
            int currentExpandCount = 0)
        {
            _lookupAttributeName = lookupAttributeName;
            _lookupDisplayName = lookupDisplayName;
            _targetTableLogicalName = targetTableLogicalName;
            _targetTableDisplayName = targetTableDisplayName;
            _targetTablePrimaryKey = targetTablePrimaryKey;
            _forms = forms;
            _allAttributes = allAttributes;
            _existingSelection = existingSelection;
            _existingFormId = existingFormId;
            _existingIncludeRelatedRecordLink = existingIncludeRelatedRecordLink;
            _currentExpandCount = currentExpandCount;

            InitializeComponent();
            PopulateFormDropdown();
        }

        private void InitializeComponent()
        {
            this.Text = $"Expand Lookup - {_lookupDisplayName}";
            this.Size = new Size(600, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int y = 15;

            // Lookup info header
            lblLookupInfo = new WinLabel
            {
                Text = $"Lookup Field: {_lookupDisplayName} ({_lookupAttributeName})",
                Location = new Point(CONTENT_LEFT, y),
                AutoSize = true,
                Font = new Font(this.Font, FontStyle.Bold)
            };
            this.Controls.Add(lblLookupInfo);
            y += 25;

            lblTargetTable = new WinLabel
            {
                Text = $"Related Table: {_targetTableDisplayName} ({_targetTableLogicalName})",
                Location = new Point(CONTENT_LEFT, y),
                AutoSize = true,
                ForeColor = Color.DarkBlue
            };
            this.Controls.Add(lblTargetTable);
            y += 30;

            chkIncludeRelatedRecordLink = new CheckBox
            {
                Text = "Include link to the related record using this lookup value",
                Location = new Point(CONTENT_LEFT, y),
                AutoSize = true,
                Checked = _existingIncludeRelatedRecordLink
            };
            chkIncludeRelatedRecordLink.CheckedChanged += ChkIncludeRelatedRecordLink_CheckedChanged;
            this.Controls.Add(chkIncludeRelatedRecordLink);
            y += 24;

            // Warning panel
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
            this.Controls.Add(pnlWarning);

            // Form selector
            lblForm = new WinLabel
            {
                Text = "Form:",
                Location = new Point(CONTENT_LEFT, y + 3),
                AutoSize = true
            };
            this.Controls.Add(lblForm);

            cboForm = new ComboBox
            {
                Location = new Point(60, y),
                Size = new Size(380, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboForm.SelectedIndexChanged += CboForm_SelectedIndexChanged;
            this.Controls.Add(cboForm);

            lblFormFieldCount = new WinLabel
            {
                Text = "",
                Location = new Point(450, y + 3),
                AutoSize = true,
                ForeColor = Color.Gray
            };
            this.Controls.Add(lblFormFieldCount);
            y += 35;

            // Attribute list
            listViewAttributes = new ListView
            {
                Location = new Point(CONTENT_LEFT, y),
                Size = new Size(CONTENT_WIDTH, 320),
                View = View.Details,
                FullRowSelect = true,
                CheckBoxes = true
            };
            listViewAttributes.Columns.Add("Sel", 35);
            listViewAttributes.Columns.Add("Display Name", 200);
            listViewAttributes.Columns.Add("Logical Name", 170);
            listViewAttributes.Columns.Add("Type", 120);
            listViewAttributes.ColumnClick += ListViewAttributes_ColumnClick;
            listViewAttributes.ItemChecked += ListViewAttributes_ItemChecked;
            this.Controls.Add(listViewAttributes);
            y += 325;

            // Buttons row
            btnSelectAll = new Button
            {
                Text = "Select All",
                Location = new Point(15, y + 5),
                Size = new Size(80, 28)
            };
            btnSelectAll.Click += BtnSelectAll_Click;
            this.Controls.Add(btnSelectAll);

            btnDeselectAll = new Button
            {
                Text = "Deselect All",
                Location = new Point(100, y + 5),
                Size = new Size(85, 28)
            };
            btnDeselectAll.Click += BtnDeselectAll_Click;
            this.Controls.Add(btnDeselectAll);

            lblStatus = new WinLabel
            {
                Text = "Select a form to see available attributes.",
                Location = new Point(195, y + 10),
                AutoSize = false,
                Size = new Size(195, 28),
                AutoEllipsis = true,
                ForeColor = Color.Gray
            };
            this.Controls.Add(lblStatus);

            btnOk = new Button
            {
                Text = "OK",
                Location = new Point(400, y + 5),
                Size = new Size(75, 28),
                DialogResult = DialogResult.OK,
                Enabled = false
            };
            btnOk.Click += BtnOk_Click;
            this.Controls.Add(btnOk);

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(485, y + 5),
                Size = new Size(75, 28),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;

            LayoutControls();
        }

        private void LayoutControls()
        {
            int y = 15;

            lblLookupInfo.Location = new Point(CONTENT_LEFT, y);
            y += 25;

            lblTargetTable.Location = new Point(CONTENT_LEFT, y);
            y += 30;

            chkIncludeRelatedRecordLink.Location = new Point(CONTENT_LEFT, y);
            y += 24;

            pnlWarning.Location = new Point(CONTENT_LEFT, y);
            if (pnlWarning.Visible)
            {
                y += pnlWarning.Height + 5;
            }

            lblForm.Location = new Point(CONTENT_LEFT, y + 3);
            cboForm.Location = new Point(60, y);
            lblFormFieldCount.Location = new Point(450, y + 3);
            y += 35;

            int buttonTop = this.ClientSize.Height - BUTTON_ROW_HEIGHT - 15;
            int listHeight = Math.Max(220, buttonTop - y - 10);

            listViewAttributes.Location = new Point(CONTENT_LEFT, y);
            listViewAttributes.Size = new Size(CONTENT_WIDTH, listHeight);

            btnSelectAll.Location = new Point(CONTENT_LEFT, buttonTop);
            btnDeselectAll.Location = new Point(100, buttonTop);
            lblStatus.Location = new Point(195, buttonTop + 5);
            btnOk.Location = new Point(400, buttonTop);
            btnCancel.Location = new Point(485, buttonTop);
            lblStatus.Size = new Size(Math.Max(80, btnOk.Left - lblStatus.Left - 10), BUTTON_ROW_HEIGHT);
        }

        private void PopulateFormDropdown()
        {
            cboForm.Items.Clear();
            cboForm.Items.Add("(All Attributes)");

            foreach (var form in _forms.OrderBy(f => f.Name))
            {
                var fieldCount = form.Fields?.Count ?? 0;
                cboForm.Items.Add(new FormComboItem(form, fieldCount));
            }

            // Pre-select existing form or first available
            if (!string.IsNullOrEmpty(_existingFormId))
            {
                for (int i = 1; i < cboForm.Items.Count; i++)
                {
                    if (cboForm.Items[i] is FormComboItem item && item.Form.FormId == _existingFormId)
                    {
                        cboForm.SelectedIndex = i;
                        return;
                    }
                }
            }

            // Default to first form if available, otherwise "All Attributes"
            cboForm.SelectedIndex = cboForm.Items.Count > 1 ? 1 : 0;
        }

        private void CboForm_SelectedIndexChanged(object? sender, EventArgs e)
        {
            PopulateAttributes();
        }

        private void ListViewAttributes_ColumnClick(object? sender, ColumnClickEventArgs e)
        {
            if (e.Column == _sortColumn)
            {
                _sortAscending = !_sortAscending;
            }
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

            // Get form fields filter
            HashSet<string>? formFields = null;
            if (cboForm.SelectedItem is FormComboItem formItem)
            {
                formFields = formItem.Form.Fields != null
                    ? new HashSet<string>(formItem.Form.Fields, StringComparer.OrdinalIgnoreCase)
                    : null;
                lblFormFieldCount.Text = formFields != null ? $"{formFields.Count} fields" : "";
                SelectedFormId = formItem.Form.FormId;
            }
            else
            {
                lblFormFieldCount.Text = $"{_allAttributes.Count} total";
                SelectedFormId = null;
            }

            // Build existing selection lookup
            var existingSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (_existingSelection != null)
            {
                foreach (var attr in _existingSelection)
                    existingSet.Add(attr.LogicalName);
            }

            // Filter and display attributes
            var filteredAttrs = _allAttributes
                .Where(a => !IsExcludedAttribute(a))
                .Where(a => formFields == null || formFields.Contains(a.LogicalName) || existingSet.Contains(a.LogicalName))
                .OrderBy(a => a.DisplayName ?? a.LogicalName)
                .ToList();

            foreach (var attr in filteredAttrs)
            {
                var item = new ListViewItem("");
                item.Checked = existingSet.Contains(attr.LogicalName);
                item.SubItems.Add(attr.DisplayName ?? attr.LogicalName);
                item.SubItems.Add(attr.LogicalName);
                item.SubItems.Add(attr.AttributeType ?? "");
                item.Tag = attr;
                item.Name = attr.LogicalName;

                // Gray out attributes not on the form
                if (formFields != null && !formFields.Contains(attr.LogicalName) && !existingSet.Contains(attr.LogicalName))
                {
                    item.ForeColor = Color.LightGray;
                }

                listViewAttributes.Items.Add(item);
            }

            ApplyListSort();
            listViewAttributes.EndUpdate();
            _isLoading = false;
            UpdateStatus();
            UpdateWarnings();
        }

        private void ApplyListSort()
        {
            if (listViewAttributes.Items.Count == 0)
                return;

            listViewAttributes.ListViewItemSorter = new AttributeListViewItemComparer(_sortColumn, _sortAscending);
            listViewAttributes.Sort();
        }

        /// <summary>
        /// Excludes system/virtual attributes that shouldn't be expanded.
        /// </summary>
        private bool IsExcludedAttribute(CoreAttributeMetadata attr)
        {
            // Exclude virtual attributes
            if (attr.AttributeType?.Equals("Virtual", StringComparison.OrdinalIgnoreCase) == true)
                return true;

            // Exclude primary key (it's used for the JOIN, not as a display column)
            if (attr.LogicalName.Equals(_targetTablePrimaryKey, StringComparison.OrdinalIgnoreCase))
                return true;

            // Exclude state/status codes
            if (attr.LogicalName.Equals("statecode", StringComparison.OrdinalIgnoreCase) ||
                attr.LogicalName.Equals("statuscode", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        private void ListViewAttributes_ItemChecked(object? sender, ItemCheckedEventArgs e)
        {
            if (_isLoading) return;
            UpdateStatus();
            UpdateWarnings();
        }

        private void UpdateStatus()
        {
            var checkedCount = listViewAttributes.CheckedItems.Count;
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
            var checkedCount = listViewAttributes.CheckedItems.Count;
            var warnings = new List<string>();

            if (checkedCount >= MAX_RECOMMENDED_FIELDS)
            {
                warnings.Add($"\u26a0 {checkedCount} fields selected. Selecting {MAX_RECOMMENDED_FIELDS}+ fields from a single expanded lookup may impact DirectQuery performance.");
            }

            // +1 because adding this expand counts toward the total
            var totalExpands = _currentExpandCount + (checkedCount > 0 ? 1 : 0);
            if (totalExpands >= MAX_RECOMMENDED_EXPANDS)
            {
                warnings.Add($"\u26a0 This table has {totalExpands} expanded lookups. {MAX_RECOMMENDED_EXPANDS}+ expanded lookups on a single table may impact performance.");
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

        private void BtnSelectAll_Click(object? sender, EventArgs e)
        {
            _isLoading = true;
            foreach (ListViewItem item in listViewAttributes.Items)
            {
                item.Checked = true;
            }
            _isLoading = false;
            UpdateStatus();
            UpdateWarnings();
        }

        private void BtnDeselectAll_Click(object? sender, EventArgs e)
        {
            _isLoading = true;
            foreach (ListViewItem item in listViewAttributes.Items)
            {
                item.Checked = false;
            }
            _isLoading = false;
            UpdateStatus();
            UpdateWarnings();
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            SelectedAttributes = listViewAttributes.CheckedItems.Cast<ListViewItem>()
                .Where(item => item.Tag is CoreAttributeMetadata)
                .Select(item =>
                {
                    var attr = (CoreAttributeMetadata)item.Tag;
                    return new ExpandedLookupAttribute
                    {
                        LogicalName = attr.LogicalName,
                        DisplayName = attr.DisplayName,
                        AttributeType = attr.AttributeType,
                        SchemaName = attr.SchemaName,
                        Targets = attr.Targets,
                        VirtualAttributeName = attr.VirtualAttributeName,
                        IsGlobal = attr.IsGlobal,
                        OptionSetName = attr.OptionSetName,
                        IncludeInModel = true
                    };
                })
                .ToList();
        }

        /// <summary>
        /// Helper class for form combo box items.
        /// </summary>
        private class FormComboItem
        {
            public FormMetadata Form { get; }
            public int FieldCount { get; }

            public FormComboItem(FormMetadata form, int fieldCount)
            {
                Form = form;
                FieldCount = fieldCount;
            }

            public override string ToString()
            {
                return $"{Form.Name} ({FieldCount} fields)";
            }
        }

        private class AttributeListViewItemComparer : IComparer
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
