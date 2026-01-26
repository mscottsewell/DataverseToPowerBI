using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataverseMetadataExtractor.Models;
using DataverseMetadataExtractor.Services;
using Newtonsoft.Json;

namespace DataverseMetadataExtractor.Forms
{
    public partial class MainForm : Form
    {
        private DataverseClient? _client;
        private readonly SettingsManager _settingsManager;
        private AppSettings _settings;
        private MetadataCache _cache;
        
        // State management
        private Dictionary<string, TableInfo> _selectedTables = new();  // logicalName -> table
        private Dictionary<string, List<FormMetadata>> _tableForms = new();  // logicalName -> forms
        private Dictionary<string, List<ViewMetadata>> _tableViews = new();  // logicalName -> views
        private Dictionary<string, List<AttributeMetadata>> _tableAttributes = new();  // logicalName -> attributes
        private Dictionary<string, HashSet<string>> _selectedAttributes = new();  // logicalName -> selected attr names
        private Dictionary<string, string> _selectedViews = new();  // logicalName -> selected view id
        private Dictionary<string, bool> _loadingStates = new();  // logicalName -> is loading
        
        // Sorting state
        private int _selectedTablesSortColumn = -1;
        private bool _selectedTablesSortAscending = true;
        private int _attributesSortColumn = -1;
        private bool _attributesSortAscending = true;
        
        private string? _currentSolutionName;
        private bool _isLoading = false;  // Prevent SaveSettings during initial load

        public MainForm()
        {
            InitializeComponent();
            _settingsManager = new SettingsManager();
            _settings = _settingsManager.LoadSettings();
            _cache = _settingsManager.LoadCache() ?? new MetadataCache();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _isLoading = true;  // Prevent SaveSettings during initialization
            
            // Disable connection-dependent controls
            btnSelectTables.Enabled = false;
            EnableMetadataDependentControls(false);
            
            try
            {
                // Restore settings (strip https:// from URL for display)
                var url = _settings.LastEnvironmentUrl ?? "";
                if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    url = url.Substring(8);
                txtEnvironmentUrl.Text = url;
                
                txtProjectName.Text = _settings.ProjectName ?? "";
                txtOutputFolder.Text = _settings.OutputFolder ?? "";
                
                // Restore radio button state (only set one since they're mutually exclusive)
                if (_settings.ShowAllAttributes)
                    radioShowAll.Checked = true;
                else
                    radioShowSelected.Checked = true;

                // Restore current solution name
                _currentSolutionName = _settings.LastSolution;

                // Restore from cache if valid
                if (!string.IsNullOrEmpty(_settings.LastEnvironmentUrl) && 
                    !string.IsNullOrEmpty(_settings.LastSolution) &&
                    _cache.IsValidFor(_settings.LastEnvironmentUrl, _settings.LastSolution))
                {
                    SetStatus($"Loaded cache from {_cache.CachedDate:g}");
                    RestoreFromCache();
                }
                else if (_settings.SelectedTables.Any())
                {
                    // Cache is invalid but we have settings - restore minimal state without metadata
                    RestoreFromSettings();
                }
                
                UpdateTableCount();
            }
            finally
            {
                _isLoading = false;  // Re-enable SaveSettings
            }
            
            // Save settings if we populated display info from cache
            if (_settings.TableDisplayInfo.Any() || _settings.AttributeDisplayInfo.Any())
            {
                SaveSettings();
            }
        }

        private void RestoreFromSettings()
        {
            // Restore minimal state from settings (without full metadata)
            // This prevents SaveSettings() from clearing everything when cache is expired
            
            foreach (var tableName in _settings.SelectedTables)
            {
                // Create TableInfo from saved display info or use logical name as fallback
                var tableInfo = new TableInfo
                {
                    LogicalName = tableName,
                    DisplayName = _settings.TableDisplayInfo.ContainsKey(tableName) 
                        ? _settings.TableDisplayInfo[tableName].DisplayName ?? tableName
                        : tableName,
                    SchemaName = _settings.TableDisplayInfo.ContainsKey(tableName)
                        ? _settings.TableDisplayInfo[tableName].SchemaName
                        : tableName,
                    PrimaryIdAttribute = _settings.TableDisplayInfo.ContainsKey(tableName)
                        ? _settings.TableDisplayInfo[tableName].PrimaryIdAttribute
                        : null,
                    PrimaryNameAttribute = _settings.TableDisplayInfo.ContainsKey(tableName)
                        ? _settings.TableDisplayInfo[tableName].PrimaryNameAttribute
                        : null
                };
                
                _selectedTables[tableName] = tableInfo;
                
                // Restore selected attributes
                if (_settings.TableAttributes.ContainsKey(tableName))
                {
                    _selectedAttributes[tableName] = new HashSet<string>(_settings.TableAttributes[tableName]);
                    
                    // Create minimal AttributeMetadata for display purposes
                    var attrList = new List<AttributeMetadata>();
                    foreach (var attrName in _settings.TableAttributes[tableName])
                    {
                        // Check if we have saved display info
                        if (_settings.AttributeDisplayInfo.ContainsKey(tableName) &&
                            _settings.AttributeDisplayInfo[tableName].ContainsKey(attrName))
                        {
                            var info = _settings.AttributeDisplayInfo[tableName][attrName];
                            attrList.Add(new AttributeMetadata
                            {
                                LogicalName = info.LogicalName,
                                DisplayName = info.DisplayName,
                                SchemaName = info.SchemaName,
                                AttributeType = info.AttributeType
                            });
                        }
                        else
                        {
                            // Fallback: create minimal metadata with just logical name
                            attrList.Add(new AttributeMetadata
                            {
                                LogicalName = attrName,
                                DisplayName = attrName,
                                SchemaName = attrName,
                                AttributeType = "(reconnect to load)"
                            });
                        }
                    }
                    _tableAttributes[tableName] = attrList;
                }
                
                // Restore selected view IDs
                if (_settings.TableViews.ContainsKey(tableName))
                {
                    _selectedViews[tableName] = _settings.TableViews[tableName];
                }
                
                // Add to UI with display name
                var formName = _settings.TableFormNames.ContainsKey(tableName)
                    ? _settings.TableFormNames[tableName]
                    : "(reconnect to load)";
                var viewName = _settings.TableViewNames.ContainsKey(tableName)
                    ? _settings.TableViewNames[tableName]
                    : "(reconnect to load)";
                    
                var item = new ListViewItem("✏️");  // Edit column
                item.Name = tableName;
                item.SubItems.Add(tableInfo.DisplayName ?? tableName);  // Table column
                item.SubItems.Add(formName);  // Form column
                item.SubItems.Add(viewName);  // View column
                item.SubItems.Add(_settings.TableAttributes.ContainsKey(tableName) 
                    ? _settings.TableAttributes[tableName].Count.ToString() 
                    : "0");  // Attrs column
                listViewSelectedTables.Items.Add(item);
            }
            
            // Auto-select first table
            if (listViewSelectedTables.Items.Count > 0)
            {
                listViewSelectedTables.Items[0].Selected = true;
            }
            
            SetStatus("Previous session restored. Please reconnect to refresh metadata.");
        }

        private void RestoreFromCache()
        {
            _currentSolutionName = _cache.SolutionName;

            // Restore tables from cache
            foreach (var kvp in _cache.TableData)
            {
                var logicalName = kvp.Key;
                var tableInfo = kvp.Value;
                
                _selectedTables[logicalName] = tableInfo;
                
                // Save display info to settings for persistence
                _settings.TableDisplayInfo[logicalName] = new TableDisplayInfo
                {
                    LogicalName = tableInfo.LogicalName,
                    DisplayName = tableInfo.DisplayName,
                    SchemaName = tableInfo.SchemaName,
                    PrimaryIdAttribute = tableInfo.PrimaryIdAttribute,
                    PrimaryNameAttribute = tableInfo.PrimaryNameAttribute
                };
                
                // Restore cached metadata
                if (_cache.TableForms.ContainsKey(logicalName))
                    _tableForms[logicalName] = _cache.TableForms[logicalName];
                
                if (_cache.TableViews.ContainsKey(logicalName))
                    _tableViews[logicalName] = _cache.TableViews[logicalName];
                
                if (_cache.TableAttributes.ContainsKey(logicalName))
                {
                    _tableAttributes[logicalName] = _cache.TableAttributes[logicalName];
                    
                    // Save attribute display info to settings
                    var requiredAttrs = GetRequiredAttributes(tableInfo);
                    var attrDict = new Dictionary<string, AttributeDisplayInfo>();
                    foreach (var attr in _cache.TableAttributes[logicalName])
                    {
                        attrDict[attr.LogicalName] = new AttributeDisplayInfo
                        {
                            LogicalName = attr.LogicalName,
                            DisplayName = attr.DisplayName,
                            SchemaName = attr.SchemaName,
                            AttributeType = attr.AttributeType,
                            IsRequired = requiredAttrs.Contains(attr.LogicalName)
                        };
                    }
                    _settings.AttributeDisplayInfo[logicalName] = attrDict;
                }
                
                // Restore attribute selections
                var savedAttrs = _settings.TableAttributes.ContainsKey(logicalName) 
                    ? new HashSet<string>(_settings.TableAttributes[logicalName])
                    : new HashSet<string>();
                
                // Always include required attributes
                var required = GetRequiredAttributes(tableInfo);
                savedAttrs.UnionWith(required);
                _selectedAttributes[logicalName] = savedAttrs;
                
                // Restore view selection
                if (_settings.TableViews.ContainsKey(logicalName))
                {
                    _selectedViews[logicalName] = _settings.TableViews[logicalName];
                }
                else if (_tableViews.ContainsKey(logicalName))
                {
                    var views = _tableViews[logicalName];
                    var defaultView = views.FirstOrDefault(v => v.IsDefault) ?? views.FirstOrDefault();
                    if (defaultView != null)
                        _selectedViews[logicalName] = defaultView.ViewId;
                }
                
                // Add to UI
                AddTableToSelectedList(tableInfo);
            }
            
            UpdateTableCount();
            
            // Auto-select first table
            if (listViewSelectedTables.Items.Count > 0)
            {
                listViewSelectedTables.Items[0].Selected = true;
            }
        }

        private HashSet<string> GetRequiredAttributes(TableInfo table)
        {
            var required = new HashSet<string>();
            if (!string.IsNullOrEmpty(table.PrimaryIdAttribute))
                required.Add(table.PrimaryIdAttribute);
            if (!string.IsNullOrEmpty(table.PrimaryNameAttribute))
                required.Add(table.PrimaryNameAttribute);
            return required;
        }

        private void EnableMetadataDependentControls(bool enabled)
        {
            btnExport.Enabled = enabled;
            btnSelectFromForm.Enabled = enabled;
            radioShowAll.Enabled = enabled;
            
            // Force "Selected" radio button when disabling
            if (!enabled && !radioShowSelected.Checked)
            {
                radioShowSelected.Checked = true;
            }
        }

        private void AutoSelectFormFields(string logicalName, FormMetadata form)
        {
            if (!_selectedAttributes.ContainsKey(logicalName) || 
                !_tableAttributes.ContainsKey(logicalName))
                return;

            var selectedAttrs = _selectedAttributes[logicalName];
            var attributes = _tableAttributes[logicalName];
            var requiredAttrs = GetRequiredAttributes(_selectedTables[logicalName]);

            // Clear current selections
            selectedAttrs.Clear();

            // Always include required attributes (primary ID and name)
            foreach (var req in requiredAttrs)
                selectedAttrs.Add(req);

            // Add all fields from the form
            if (form.Fields != null)
            {
                foreach (var field in form.Fields)
                {
                    if (attributes.Any(a => a.LogicalName.Equals(field, StringComparison.OrdinalIgnoreCase)))
                        selectedAttrs.Add(field);
                }
            }
        }

        private void SaveSettings()
        {
            if (_isLoading) return;  // Don't save during initial load
            
            // Store URL without https:// prefix
            var url = txtEnvironmentUrl.Text.Trim();
            if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                url = url.Substring(8);
            
            _settings.LastEnvironmentUrl = url;
            _settings.LastSolution = _currentSolutionName ?? "";
            _settings.SelectedTables = _selectedTables.Keys.ToList();
            _settings.TableForms = GetSelectedFormIds();
            _settings.TableFormNames = GetSelectedFormNames();
            _settings.TableViews = _selectedViews.ToDictionary(k => k.Key, v => v.Value);
            _settings.TableViewNames = GetSelectedViewNames();
            _settings.TableAttributes = _selectedAttributes.ToDictionary(
                k => k.Key,
                v => v.Value.ToList()
            );
            
            // Save table display info
            _settings.TableDisplayInfo = _selectedTables.ToDictionary(
                k => k.Key,
                v => new TableDisplayInfo
                {
                    LogicalName = v.Value.LogicalName,
                    DisplayName = v.Value.DisplayName,
                    SchemaName = v.Value.SchemaName,
                    PrimaryIdAttribute = v.Value.PrimaryIdAttribute,
                    PrimaryNameAttribute = v.Value.PrimaryNameAttribute
                }
            );
            
            // Save attribute display info
            _settings.AttributeDisplayInfo = new Dictionary<string, Dictionary<string, AttributeDisplayInfo>>();
            foreach (var tableName in _selectedAttributes.Keys)
            {
                if (!_tableAttributes.ContainsKey(tableName))
                    continue;
                
                if (!_selectedTables.ContainsKey(tableName))
                    continue;
                
                var requiredAttrs = GetRequiredAttributes(_selectedTables[tableName]);
                var attrDict = new Dictionary<string, AttributeDisplayInfo>();
                foreach (var attrLogicalName in _selectedAttributes[tableName])
                {
                    var attrMeta = _tableAttributes[tableName].FirstOrDefault(a => a.LogicalName == attrLogicalName);
                    if (attrMeta != null)
                    {
                        attrDict[attrLogicalName] = new AttributeDisplayInfo
                        {
                            LogicalName = attrMeta.LogicalName,
                            DisplayName = attrMeta.DisplayName,
                            SchemaName = attrMeta.SchemaName,
                            AttributeType = attrMeta.AttributeType,
                            IsRequired = requiredAttrs.Contains(attrLogicalName)
                        };
                    }
                }
                if (attrDict.Any())
                    _settings.AttributeDisplayInfo[tableName] = attrDict;
            }
            
            _settings.ProjectName = txtProjectName.Text;
            _settings.OutputFolder = txtOutputFolder.Text;
            _settings.WindowGeometry = $"{Width},{Height}";
            _settings.ShowAllAttributes = radioShowAll.Checked;
            
            _settingsManager.SaveSettings(_settings);
        }

        private Dictionary<string, string> GetSelectedFormIds()
        {
            var result = new Dictionary<string, string>();
            foreach (ListViewItem item in listViewSelectedTables.Items)
            {
                var logicalName = item.Name;
                var formName = item.SubItems[1].Text;
                
                if (_tableForms.ContainsKey(logicalName))
                {
                    var form = _tableForms[logicalName].FirstOrDefault(f => f.Name == formName);
                    if (form != null)
                        result[logicalName] = form.FormId;
                }
            }
            return result;
        }
        
        private Dictionary<string, string> GetSelectedFormNames()
        {
            var result = new Dictionary<string, string>();
            foreach (ListViewItem item in listViewSelectedTables.Items)
            {
                var logicalName = item.Name;
                var formName = item.SubItems[1].Text;
                
                // Only save if it's not a placeholder
                if (!formName.Contains("loading") && !formName.Contains("not loaded") && 
                    !formName.Contains("no forms") && !formName.Contains("reconnect"))
                {
                    result[logicalName] = formName;
                }
            }
            return result;
        }
        
        private Dictionary<string, string> GetSelectedViewNames()
        {
            var result = new Dictionary<string, string>();
            foreach (ListViewItem item in listViewSelectedTables.Items)
            {
                var logicalName = item.Name;
                var viewName = item.SubItems[2].Text;
                
                // Only save if it's not a placeholder
                if (!viewName.Contains("loading") && !viewName.Contains("not loaded") && 
                    !viewName.Contains("no views") && !viewName.Contains("reconnect"))
                {
                    result[logicalName] = viewName;
                }
            }
            return result;
        }

        private void SaveCache()
        {
            var envUrl = txtEnvironmentUrl.Text.Trim();
            if (string.IsNullOrEmpty(envUrl) || string.IsNullOrEmpty(_currentSolutionName))
                return;
            
            // Add https:// prefix for cache storage (consistency with validation)
            if (!envUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                envUrl = "https://" + envUrl;
            
            _cache.EnvironmentUrl = envUrl;
            _cache.SolutionName = _currentSolutionName;
            _cache.CachedDate = DateTime.Now;
            _cache.TableData = _selectedTables.ToDictionary(k => k.Key, v => v.Value);
            _cache.TableForms = _tableForms.ToDictionary(k => k.Key, v => v.Value);
            _cache.TableViews = _tableViews.ToDictionary(k => k.Key, v => v.Value);
            _cache.TableAttributes = _tableAttributes.ToDictionary(k => k.Key, v => v.Value);
            
            _settingsManager.SaveCache(_cache);
        }

        private void UpdateTableCount()
        {
            var count = _selectedTables.Count;
            lblTableCount.Text = count == 0 ? "No tables selected" :
                                count == 1 ? "1 table selected" :
                                $"{count} tables selected";
        }

        private void SetStatus(string message)
        {
            lblStatus.Text = message;
            Application.DoEvents();
        }

        private void ShowProgress(bool show)
        {
            progressBar.Visible = show;
        }

        private async void BtnConnect_Click(object sender, EventArgs e)
        {
            var url = txtEnvironmentUrl.Text.Trim();
            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Please enter an environment URL.", "Validation", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Add https:// for connection (but don't update the text box)
            var connectionUrl = url;
            if (!connectionUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                connectionUrl = "https://" + connectionUrl;

            try
            {
                btnConnect.Enabled = false;
                lblConnectionStatus.Text = "Connecting...";
                SetStatus("Authenticating to Dataverse...");
                ShowProgress(true);

                _client = new DataverseClient(connectionUrl);
                await _client.AuthenticateAsync();

                lblConnectionStatus.Text = "Connected";
                btnSelectTables.Enabled = true;  // Enable table selection
                SetStatus("Connected successfully.");
                SaveSettings();
                
                // Revalidate cached metadata if we have any selected tables
                if (_selectedTables.Any())
                {
                    await RevalidateMetadata();
                }
            }
            catch (Exception ex)
            {
                lblConnectionStatus.Text = "Connection failed";
                btnSelectTables.Enabled = false;  // Disable table selection
                MessageBox.Show($"Connection failed:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("Connection failed.");
            }
            finally
            {
                btnConnect.Enabled = true;
                ShowProgress(false);
            }
        }

        private void BtnSelectTables_Click(object sender, EventArgs e)
        {
            if (_client == null)
            {
                MessageBox.Show("Please connect first.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Show table selector dialog with currently selected tables
            var currentlySelectedTables = _selectedTables.Keys.ToList();
            using var dialog = new TableSelectorDialog(_client, _currentSolutionName, currentlySelectedTables);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _currentSolutionName = dialog.SelectedSolutionName;
                var selectedTables = dialog.SelectedTables;
                
                if (selectedTables.Any())
                {
                    AddTablesInBulk(selectedTables);
                }
            }
        }

        private async void AddTablesInBulk(List<TableInfo> tables)
        {
            // Add all tables to the list first
            foreach (var table in tables)
            {
                var logicalName = table.LogicalName;
                if (_selectedTables.ContainsKey(logicalName))
                    continue;  // Already added
                
                _selectedTables[logicalName] = table;
                _selectedAttributes[logicalName] = GetRequiredAttributes(table);
                _loadingStates[logicalName] = true;
                
                AddTableToSelectedList(table, loading: true);
            }
            
            UpdateTableCount();
            SaveSettings();

            // Now load metadata in parallel
            SetStatus($"Loading metadata for {tables.Count} tables...");
            ShowProgress(true);

            var tasks = tables.Select(t => LoadTableMetadata(t.LogicalName)).ToList();
            await Task.WhenAll(tasks);

            ShowProgress(false);
            SetStatus($"Loaded metadata for {tables.Count} tables.");
            SaveCache();
            
            // Enable metadata-dependent controls after successful load
            EnableMetadataDependentControls(true);

            // Auto-select first table if nothing selected
            if (listViewSelectedTables.SelectedItems.Count == 0 && listViewSelectedTables.Items.Count > 0)
            {
                listViewSelectedTables.Items[0].Selected = true;
            }
        }

        private async Task LoadTableMetadata(string logicalName)
        {
            if (_client == null) return;

            try
            {
                // Load attributes, forms, and views in parallel
                var attrTask = _client.GetAttributesAsync(logicalName);
                var formsTask = _client.GetFormsAsync(logicalName, includeXml: true);
                var viewsTask = _client.GetViewsAsync(logicalName);

                await Task.WhenAll(attrTask, formsTask, viewsTask);

                var attributes = await attrTask;
                var forms = await formsTask;
                var views = await viewsTask;

                // Update state
                _tableAttributes[logicalName] = attributes;
                _tableForms[logicalName] = forms;
                _tableViews[logicalName] = views;

                // Auto-select attributes based on form (for new tables only, not when restoring from cache)
                var selectedAttrs = _selectedAttributes[logicalName];
                var requiredAttrs = GetRequiredAttributes(_selectedTables[logicalName]);
                
                // Check if this is a new table (not restored from cache) by seeing if only required attrs are selected
                bool isNewTable = selectedAttrs.Count == requiredAttrs.Count && requiredAttrs.All(r => selectedAttrs.Contains(r));
                
                if (isNewTable && forms.Any())
                {
                    // Get the form that will be used (from settings or default to first)
                    var formId = _settings.TableForms.ContainsKey(logicalName) 
                        ? _settings.TableForms[logicalName] 
                        : forms.First().FormId;
                    
                    var selectedForm = forms.FirstOrDefault(f => f.FormId == formId) ?? forms.First();
                    
                    // Clear current selections (except required) and select form fields
                    selectedAttrs.Clear();
                    
                    // Always include required attributes (primary ID and name)
                    foreach (var req in requiredAttrs)
                        selectedAttrs.Add(req);
                    
                    // Add all fields from the selected form
                    if (selectedForm.Fields != null)
                    {
                        foreach (var field in selectedForm.Fields)
                        {
                            if (attributes.Any(a => a.LogicalName.Equals(field, StringComparison.OrdinalIgnoreCase)))
                                selectedAttrs.Add(field);
                        }
                    }
                }

                // Select default view
                if (views.Any() && !_selectedViews.ContainsKey(logicalName))
                {
                    var defaultView = views.FirstOrDefault(v => v.IsDefault) ?? views.First();
                    _selectedViews[logicalName] = defaultView.ViewId;
                }

                // Update UI on main thread
                this.Invoke((MethodInvoker)delegate
                {
                    UpdateSelectedTableRow(logicalName);
                    _loadingStates[logicalName] = false;
                });
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    SetStatus($"Error loading metadata for {logicalName}: {ex.Message}");
                    _loadingStates[logicalName] = false;
                });
            }
        }

        private async Task RevalidateMetadata()
        {
            if (_client == null || !_selectedTables.Any())
                return;

            SetStatus($"Revalidating metadata for {_selectedTables.Count} tables...");
            ShowProgress(true);

            try
            {
                var tasks = _selectedTables.Keys.Select(logicalName => RevalidateTableMetadata(logicalName)).ToList();
                await Task.WhenAll(tasks);

                SetStatus($"Metadata revalidation complete.");
                SaveCache();
                
                // Enable metadata-dependent controls after successful revalidation
                EnableMetadataDependentControls(true);
                
                // Refresh UI for currently selected table
                if (listViewSelectedTables.SelectedItems.Count > 0)
                {
                    var selectedLogicalName = listViewSelectedTables.SelectedItems[0].Name;
                    UpdateAttributesDisplay(selectedLogicalName);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error during revalidation: {ex.Message}");
            }
            finally
            {
                ShowProgress(false);
            }
        }

        private async Task RevalidateTableMetadata(string logicalName)
        {
            if (_client == null) return;

            try
            {
                // Get fresh metadata from Dataverse (including table display name)
                var tableMetadata = await _client.GetTableMetadataAsync(logicalName);
                var attributes = await _client.GetAttributesAsync(logicalName);
                var forms = await _client.GetFormsAsync(logicalName, includeXml: true);
                var views = await _client.GetViewsAsync(logicalName);

                // Update table info with fresh display name
                if (_selectedTables.ContainsKey(logicalName))
                {
                    var existingTable = _selectedTables[logicalName];
                    existingTable.DisplayName = tableMetadata.DisplayName;
                    existingTable.SchemaName = tableMetadata.SchemaName;
                    existingTable.PrimaryIdAttribute = tableMetadata.PrimaryIdAttribute;
                    existingTable.PrimaryNameAttribute = tableMetadata.PrimaryNameAttribute;
                }

                // Update cached metadata
                _tableAttributes[logicalName] = attributes;
                _tableForms[logicalName] = forms;
                _tableViews[logicalName] = views;

                // Get all fields from all forms
                var allFormFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var form in forms)
                {
                    if (form.Fields != null)
                    {
                        foreach (var field in form.Fields)
                            allFormFields.Add(field);
                    }
                }

                // Get current attribute names from Dataverse
                var currentAttributeNames = new HashSet<string>(
                    attributes.Select(a => a.LogicalName),
                    StringComparer.OrdinalIgnoreCase
                );

                // Get required attributes
                var requiredAttrs = GetRequiredAttributes(_selectedTables[logicalName]);

                // Update selected attributes
                var selectedAttrs = _selectedAttributes[logicalName];
                
                // Remove deleted attributes (attributes that no longer exist in the table)
                var toRemove = selectedAttrs
                    .Where(attr => !currentAttributeNames.Contains(attr))
                    .ToList();
                
                foreach (var attr in toRemove)
                    selectedAttrs.Remove(attr);

                // Add new attributes that are in forms but not yet selected
                foreach (var formField in allFormFields)
                {
                    if (currentAttributeNames.Contains(formField) && !selectedAttrs.Contains(formField))
                    {
                        selectedAttrs.Add(formField);
                    }
                }

                // Always ensure required attributes are included
                foreach (var req in requiredAttrs)
                {
                    if (!selectedAttrs.Contains(req))
                        selectedAttrs.Add(req);
                }

                // Update UI on main thread
                this.Invoke((MethodInvoker)delegate
                {
                    UpdateSelectedTableRow(logicalName);
                });
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    SetStatus($"Error revalidating metadata for {logicalName}: {ex.Message}");
                });
            }
        }

        private void AddTableToSelectedList(TableInfo table, bool loading = false)
        {
            var logicalName = table.LogicalName;
            var formText = loading ? "(loading...)" : GetFormDisplayText(logicalName);
            var viewText = loading ? "(loading...)" : GetViewDisplayText(logicalName);
            var attrCount = _selectedAttributes.ContainsKey(logicalName) 
                ? _selectedAttributes[logicalName].Count.ToString()
                : "0";

            var item = new ListViewItem("✏️");  // Edit column
            item.Name = logicalName;
            item.SubItems.Add(table.DisplayName ?? logicalName);  // Table column
            item.SubItems.Add(formText);  // Form column
            item.SubItems.Add(viewText);  // View column
            item.SubItems.Add(attrCount);  // Attrs column

            listViewSelectedTables.Items.Add(item);
        }

        private void UpdateSelectedTableRow(string logicalName)
        {
            var item = listViewSelectedTables.Items.Cast<ListViewItem>()
                .FirstOrDefault(i => i.Name == logicalName);
            
            if (item != null)
            {
                // Column indices: 0=Edit, 1=Table, 2=Form, 3=View, 4=Attrs
                item.SubItems[2].Text = GetFormDisplayText(logicalName);
                item.SubItems[3].Text = GetViewDisplayText(logicalName);
                item.SubItems[4].Text = _selectedAttributes.ContainsKey(logicalName)
                    ? _selectedAttributes[logicalName].Count.ToString()
                    : "0";
            }
        }

        private string GetFormDisplayText(string logicalName)
        {
            if (!_tableForms.ContainsKey(logicalName))
                return "(not loaded)";
            
            var forms = _tableForms[logicalName];
            if (!forms.Any())
                return "(no forms)";
            
            var selectedFormId = _settings.TableForms.ContainsKey(logicalName)
                ? _settings.TableForms[logicalName]
                : forms.First().FormId;
            
            var form = forms.FirstOrDefault(f => f.FormId == selectedFormId) ?? forms.First();
            return form.Name;
        }

        private string GetViewDisplayText(string logicalName)
        {
            if (!_tableViews.ContainsKey(logicalName))
                return "(not loaded)";
            
            var views = _tableViews[logicalName];
            if (!views.Any())
                return "(no views)";
            
            var selectedViewId = _selectedViews.ContainsKey(logicalName)
                ? _selectedViews[logicalName]
                : (views.FirstOrDefault(v => v.IsDefault) ?? views.First()).ViewId;
            
            var view = views.FirstOrDefault(v => v.ViewId == selectedViewId);
            return view != null ? view.Name : views.First().Name;
        }

        private void BtnRemoveTable_Click(object sender, EventArgs e)
        {
            if (listViewSelectedTables.SelectedItems.Count == 0) return;

            var item = listViewSelectedTables.SelectedItems[0];
            var logicalName = item.Name;

            _selectedTables.Remove(logicalName);
            _tableForms.Remove(logicalName);
            _tableViews.Remove(logicalName);
            _tableAttributes.Remove(logicalName);
            _selectedAttributes.Remove(logicalName);
            _selectedViews.Remove(logicalName);
            _loadingStates.Remove(logicalName);

            listViewSelectedTables.Items.Remove(item);
            listViewAttributes.Items.Clear();

            UpdateTableCount();
            SaveSettings();
            SaveCache();
        }

        private void ListViewSelectedTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewSelectedTables.SelectedItems.Count == 0)
            {
                listViewAttributes.Items.Clear();
                return;
            }

            var logicalName = listViewSelectedTables.SelectedItems[0].Name;
            UpdateAttributesDisplay(logicalName);
        }

        private void ListViewSelectedTables_Click(object sender, EventArgs e)
        {
            var info = listViewSelectedTables.HitTest(listViewSelectedTables.PointToClient(Cursor.Position));
            if (info.Item != null && info.SubItem != null)
            {
                // Check if click was on the Edit column (first column, index 0)
                if (info.Item.SubItems.IndexOf(info.SubItem) == 0)
                {
                    var logicalName = info.Item.Name;
                    ShowFormViewSelector(logicalName);
                }
            }
        }

        private void ListViewSelectedTables_DoubleClick(object sender, EventArgs e)
        {
            if (listViewSelectedTables.SelectedItems.Count == 0) return;

            var logicalName = listViewSelectedTables.SelectedItems[0].Name;
            ShowFormViewSelector(logicalName);
        }

        private void ShowFormViewSelector(string logicalName)
        {
            if (!_tableForms.ContainsKey(logicalName) || !_tableViews.ContainsKey(logicalName))
            {
                MessageBox.Show("Metadata is still loading. Please wait.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Get current selections as objects
            var currentFormId = GetSelectedFormId(logicalName);
            var currentViewId = _selectedViews.ContainsKey(logicalName) ? _selectedViews[logicalName] : null;
            
            var currentForm = currentFormId != null && _tableForms.ContainsKey(logicalName)
                ? _tableForms[logicalName].FirstOrDefault(f => f.FormId == currentFormId)
                : null;
            
            var currentView = currentViewId != null && _tableViews.ContainsKey(logicalName)
                ? _tableViews[logicalName].FirstOrDefault(v => v.ViewId == currentViewId)
                : null;

            using var dialog = new FormViewSelectorDialog(
                _tableForms[logicalName],
                _tableViews[logicalName],
                currentForm,
                currentView
            );

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                bool formChanged = false;
                
                // Update selections
                if (dialog.SelectedForm != null)
                {
                    var oldFormId = currentFormId;
                    _settings.TableForms[logicalName] = dialog.SelectedForm.FormId;
                    formChanged = oldFormId != dialog.SelectedForm.FormId;
                }

                if (dialog.SelectedView != null)
                {
                    _selectedViews[logicalName] = dialog.SelectedView.ViewId;
                }

                // If form changed, auto-select fields from the new form
                if (formChanged && dialog.SelectedForm != null)
                {
                    AutoSelectFormFields(logicalName, dialog.SelectedForm);
                }

                UpdateSelectedTableRow(logicalName);
                UpdateAttributesDisplay(logicalName);
                SaveSettings();
            }
        }

        private string? GetSelectedFormId(string logicalName)
        {
            if (_settings.TableForms.ContainsKey(logicalName))
                return _settings.TableForms[logicalName];
            
            if (_tableForms.ContainsKey(logicalName) && _tableForms[logicalName].Any())
                return _tableForms[logicalName].First().FormId;
            
            return null;
        }

        private void UpdateAttributesDisplay(string logicalName)
        {
            listViewAttributes.ItemChecked -= ListViewAttributes_ItemChecked;  // Temporarily disable event
            listViewAttributes.Items.Clear();

            var selected = _selectedAttributes.ContainsKey(logicalName) 
                ? _selectedAttributes[logicalName] 
                : new HashSet<string>();
            var required = GetRequiredAttributes(_selectedTables[logicalName]);
            var filterText = txtAttrSearch.Text.ToLower();
            var showMode = radioShowAll.Checked ? "all" : "selected";

            // Get fields on form (may be empty if not loaded yet)
            var formFields = GetFormFields(logicalName);

            // If we have full attribute metadata loaded, use it
            if (_tableAttributes.ContainsKey(logicalName))
            {
                var attributes = _tableAttributes[logicalName];
                
                foreach (var attr in attributes)
                {
                    var display = attr.DisplayName ?? attr.SchemaName ?? "";
                    var logical = attr.LogicalName;
                    var isSelected = selected.Contains(logical);
                    var isRequired = required.Contains(logical);
                    var isOnForm = formFields.Contains(logical.ToLower());

                    // Apply show mode filter
                    if (showMode == "selected" && !isSelected) continue;

                    // Apply text filter
                    if (!string.IsNullOrEmpty(filterText))
                    {
                        if (!display.ToLower().Contains(filterText) && 
                            !logical.ToLower().Contains(filterText))
                            continue;
                    }

                    var item = new ListViewItem("");
                    item.Name = logical;
                    item.Checked = isSelected;
                    // Show lock for required attributes, checkmark for form fields
                    var formColumnText = isRequired ? "🔒" : (isOnForm ? "✓" : "");
                    item.SubItems.Add(formColumnText);
                    item.SubItems.Add(display);
                    item.SubItems.Add(logical);
                    item.SubItems.Add(attr.AttributeType ?? "");

                    // Apply visual styling
                    if (isRequired)
                    {
                        item.ForeColor = System.Drawing.Color.Blue;
                        item.Font = new System.Drawing.Font(item.Font, System.Drawing.FontStyle.Bold);
                    }
                    else if (isSelected)
                    {
                        item.ForeColor = System.Drawing.Color.Black;
                    }
                    else
                    {
                        item.ForeColor = System.Drawing.Color.Gray;
                    }

                    listViewAttributes.Items.Add(item);
                }
            }
            // If metadata not loaded yet but we have saved display info, show selected attributes from settings
            else if (_settings.AttributeDisplayInfo.ContainsKey(logicalName))
            {
                var savedAttrInfo = _settings.AttributeDisplayInfo[logicalName];
                
                foreach (var attrName in selected)
                {
                    var attrInfo = savedAttrInfo.ContainsKey(attrName) 
                        ? savedAttrInfo[attrName] 
                        : new AttributeDisplayInfo { LogicalName = attrName, DisplayName = attrName };
                    
                    var display = attrInfo.DisplayName ?? attrInfo.SchemaName ?? attrName;
                    // Use IsRequired from saved info or check if it's a required attribute
                    var isRequired = attrInfo.IsRequired || required.Contains(attrName);

                    // Apply text filter
                    if (!string.IsNullOrEmpty(filterText))
                    {
                        if (!display.ToLower().Contains(filterText) && 
                            !attrName.ToLower().Contains(filterText))
                            continue;
                    }

                    var item = new ListViewItem("");
                    item.Name = attrName;
                    item.Checked = true;  // All are selected in this view
                    // Show lock for required attributes (using saved IsRequired info)
                    item.SubItems.Add(isRequired ? "🔒" : "");
                    item.SubItems.Add(display);
                    item.SubItems.Add(attrName);
                    item.SubItems.Add(attrInfo.AttributeType ?? "(reconnect to load)");

                    // Apply visual styling
                    if (isRequired)
                    {
                        item.ForeColor = System.Drawing.Color.Blue;
                        item.Font = new System.Drawing.Font(item.Font, System.Drawing.FontStyle.Bold);
                    }
                    else
                    {
                        item.ForeColor = System.Drawing.Color.Black;
                    }

                    listViewAttributes.Items.Add(item);
                }
            }

            listViewAttributes.ItemChecked += ListViewAttributes_ItemChecked;  // Re-enable event
        }

        private HashSet<string> GetFormFields(string logicalName)
        {
            var fields = new HashSet<string>();
            
            if (!_tableForms.ContainsKey(logicalName))
                return fields;
            
            var formId = GetSelectedFormId(logicalName);
            if (formId == null)
                return fields;
            
            var form = _tableForms[logicalName].FirstOrDefault(f => f.FormId == formId);
            if (form?.Fields != null)
            {
                foreach (var field in form.Fields)
                    fields.Add(field.ToLower());
            }
            
            return fields;
        }

        private void ListViewAttributes_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (listViewSelectedTables.SelectedItems.Count == 0) return;

            var logicalName = listViewSelectedTables.SelectedItems[0].Name;
            var attrName = e.Item.Name;

            if (!_selectedAttributes.ContainsKey(logicalName))
                _selectedAttributes[logicalName] = new HashSet<string>();

            // Check if required
            var required = GetRequiredAttributes(_selectedTables[logicalName]);
            if (required.Contains(attrName))
            {
                e.Item.Checked = true;  // Force required attributes to stay checked
                return;
            }

            if (e.Item.Checked)
            {
                _selectedAttributes[logicalName].Add(attrName);
                // Update color to black when checked (unless it's required which stays blue/bold)
                if (!required.Contains(attrName))
                {
                    e.Item.ForeColor = System.Drawing.Color.Black;
                }
            }
            else
            {
                _selectedAttributes[logicalName].Remove(attrName);
                // Update color to gray when unchecked
                e.Item.ForeColor = System.Drawing.Color.Gray;
            }

            UpdateSelectedTableRow(logicalName);
            SaveSettings();
        }

        private void TxtAttrSearch_TextChanged(object sender, EventArgs e)
        {
            if (listViewSelectedTables.SelectedItems.Count > 0)
            {
                var logicalName = listViewSelectedTables.SelectedItems[0].Name;
                UpdateAttributesDisplay(logicalName);
            }
        }

        private void RadioShowMode_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewSelectedTables.SelectedItems.Count > 0)
            {
                var logicalName = listViewSelectedTables.SelectedItems[0].Name;
                UpdateAttributesDisplay(logicalName);
            }
        }

        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            listViewAttributes.ItemChecked -= ListViewAttributes_ItemChecked;
            
            foreach (ListViewItem item in listViewAttributes.Items)
            {
                item.Checked = true;
            }
            
            listViewAttributes.ItemChecked += ListViewAttributes_ItemChecked;

            if (listViewSelectedTables.SelectedItems.Count > 0)
            {
                var logicalName = listViewSelectedTables.SelectedItems[0].Name;
                var selected = _selectedAttributes[logicalName];
                foreach (ListViewItem item in listViewAttributes.Items)
                {
                    selected.Add(item.Name);
                }
                UpdateSelectedTableRow(logicalName);
                SaveSettings();
            }
        }

        private void BtnDeselectAll_Click(object sender, EventArgs e)
        {
            if (listViewSelectedTables.SelectedItems.Count == 0) return;

            var logicalName = listViewSelectedTables.SelectedItems[0].Name;
            var required = GetRequiredAttributes(_selectedTables[logicalName]);

            listViewAttributes.ItemChecked -= ListViewAttributes_ItemChecked;
            
            // Keep only required attributes
            _selectedAttributes[logicalName] = new HashSet<string>(required);
            
            foreach (ListViewItem item in listViewAttributes.Items)
            {
                item.Checked = required.Contains(item.Name);
            }
            
            listViewAttributes.ItemChecked += ListViewAttributes_ItemChecked;
            
            UpdateSelectedTableRow(logicalName);
            SaveSettings();
        }

        private void BtnSelectFromForm_Click(object sender, EventArgs e)
        {
            if (listViewSelectedTables.SelectedItems.Count == 0) return;

            var logicalName = listViewSelectedTables.SelectedItems[0].Name;
            var formFields = GetFormFields(logicalName);

            if (!formFields.Any())
            {
                MessageBox.Show("No form fields available. Please select a form first.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            listViewAttributes.ItemChecked -= ListViewAttributes_ItemChecked;

            var selected = _selectedAttributes[logicalName];
            var requiredAttrs = GetRequiredAttributes(_selectedTables[logicalName]);
            
            // Clear all selections
            selected.Clear();
            
            // Always include required attributes
            foreach (var req in requiredAttrs)
                selected.Add(req);
            
            // Add form fields
            foreach (var field in formFields)
            {
                selected.Add(field);
            }

            // Update checkbox states to match
            foreach (ListViewItem item in listViewAttributes.Items)
            {
                item.Checked = selected.Contains(item.Name);
            }

            listViewAttributes.ItemChecked += ListViewAttributes_ItemChecked;

            UpdateSelectedTableRow(logicalName);
            SaveSettings();
            SetStatus($"Selected {formFields.Count} fields from form.");
        }

        private void TxtProjectName_TextChanged(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void TxtOutputFolder_TextChanged(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void BtnBrowseOutput_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select Output Folder",
                SelectedPath = txtOutputFolder.Text
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                txtOutputFolder.Text = dialog.SelectedPath;
            }
        }

        private async void BtnExport_Click(object sender, EventArgs e)
        {
            if (!_selectedTables.Any())
            {
                MessageBox.Show("No tables selected.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var semanticModelName = txtProjectName.Text.Trim();
            if (string.IsNullOrEmpty(semanticModelName))
            {
                MessageBox.Show("Please enter a semantic model name.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var baseOutputFolder = txtOutputFolder.Text.Trim();
            if (string.IsNullOrEmpty(baseOutputFolder))
            {
                // Default to Reports/SemanticModelName
                var baseDir = Path.GetDirectoryName(Application.ExecutablePath);
                baseOutputFolder = Path.Combine(baseDir, "..", "..", "..", "..", "Reports");
                txtOutputFolder.Text = baseOutputFolder;
            }

            // Combine base folder with semantic model name as subfolder
            var outputFolder = Path.Combine(baseOutputFolder, semanticModelName);

            try
            {
                btnExport.Enabled = false;
                SetStatus("Preparing export...");
                ShowProgress(true);

                await Task.Run(() => ExportMetadata(semanticModelName, outputFolder));

                SetStatus($"Exported to {outputFolder}");
                ShowProgress(false);

                var totalAttrs = _selectedAttributes.Values.Sum(s => s.Count);
                MessageBox.Show(
                    $"Metadata exported successfully!\n\n" +
                    $"File: {Path.Combine(outputFolder, semanticModelName + " Metadata Dictionary.json")}\n" +
                    $"Tables: {_selectedTables.Count}\n" +
                    $"Total Attributes: {totalAttrs}",
                    "Export Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("Export failed.");
            }
            finally
            {
                btnExport.Enabled = true;
                ShowProgress(false);
            }
        }

        private void ExportMetadata(string projectName, string outputFolder)
        {
            // Add https:// prefix for export metadata
            var envUrl = txtEnvironmentUrl.Text.Trim();
            if (!envUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                envUrl = "https://" + envUrl;
            
            var metadata = new ExportMetadata
            {
                Environment = envUrl,
                Solution = _currentSolutionName ?? "",
                ProjectName = projectName,
                Tables = new List<ExportTable>()
            };

            foreach (var kvp in _selectedTables)
            {
                var logicalName = kvp.Key;
                var table = kvp.Value;

                // Get selected form
                var forms = new List<ExportForm>();
                if (_tableForms.ContainsKey(logicalName))
                {
                    var formId = GetSelectedFormId(logicalName);
                    var form = _tableForms[logicalName].FirstOrDefault(f => f.FormId == formId);
                    if (form != null)
                    {
                        forms.Add(new ExportForm
                        {
                            FormId = form.FormId,
                            FormName = form.Name,
                            FieldCount = form.Fields?.Count ?? 0
                        });
                    }
                }

                // Get selected view
                ExportView? view = null;
                if (_tableViews.ContainsKey(logicalName) && _selectedViews.ContainsKey(logicalName))
                {
                    var viewId = _selectedViews[logicalName];
                    var viewData = _tableViews[logicalName].FirstOrDefault(v => v.ViewId == viewId);
                    if (viewData != null)
                    {
                        view = new ExportView
                        {
                            ViewId = viewData.ViewId,
                            ViewName = viewData.Name,
                            FetchXml = viewData.FetchXml
                        };
                    }
                }

                // Get selected attributes
                var selectedAttrNames = _selectedAttributes.ContainsKey(logicalName)
                    ? _selectedAttributes[logicalName]
                    : new HashSet<string>();
                var allAttrs = _tableAttributes.ContainsKey(logicalName)
                    ? _tableAttributes[logicalName]
                    : new List<AttributeMetadata>();

                var attributes = allAttrs
                    .Where(a => selectedAttrNames.Contains(a.LogicalName))
                    .ToList();

                metadata.Tables.Add(new ExportTable
                {
                    LogicalName = table.LogicalName,
                    DisplayName = table.DisplayName,
                    SchemaName = table.SchemaName,
                    ObjectTypeCode = table.ObjectTypeCode,
                    PrimaryIdAttribute = table.PrimaryIdAttribute,
                    PrimaryNameAttribute = table.PrimaryNameAttribute,
                    Forms = forms,
                    View = view,
                    Attributes = attributes
                });
            }

            // Save to file
            Directory.CreateDirectory(outputFolder);
            var outputFile = Path.Combine(outputFolder, $"{projectName} Metadata Dictionary.json");

            var json = JsonConvert.SerializeObject(metadata, Formatting.Indented);
            File.WriteAllText(outputFile, json);

            // Also save DataverseURL.txt (with https:// prefix)
            var urlFile = Path.Combine(outputFolder, "DataverseURL.txt");
            var urlForFile = txtEnvironmentUrl.Text.Trim();
            if (!urlForFile.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                urlForFile = "https://" + urlForFile;
            File.WriteAllText(urlFile, urlForFile);
        }

        private void ListViewSelectedTables_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Toggle sort direction if same column, otherwise sort ascending
            if (e.Column == _selectedTablesSortColumn)
                _selectedTablesSortAscending = !_selectedTablesSortAscending;
            else
            {
                _selectedTablesSortColumn = e.Column;
                _selectedTablesSortAscending = true;
            }

            // Get all items as array
            var items = listViewSelectedTables.Items.Cast<ListViewItem>().ToArray();
            
            // Sort items
            var sorted = _selectedTablesSortAscending
                ? items.OrderBy(item => item.SubItems[e.Column].Text)
                : items.OrderByDescending(item => item.SubItems[e.Column].Text);

            // Clear and re-add sorted items
            listViewSelectedTables.Items.Clear();
            listViewSelectedTables.Items.AddRange(sorted.ToArray());
        }

        private void ListViewAttributes_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Toggle sort direction if same column, otherwise sort ascending
            if (e.Column == _attributesSortColumn)
                _attributesSortAscending = !_attributesSortAscending;
            else
            {
                _attributesSortColumn = e.Column;
                _attributesSortAscending = true;
            }

            // Temporarily disable ItemChecked event during sorting
            listViewAttributes.ItemChecked -= ListViewAttributes_ItemChecked;

            // Get all items as array
            var items = listViewAttributes.Items.Cast<ListViewItem>().ToArray();
            
            // Sort items
            var sorted = _attributesSortAscending
                ? items.OrderBy(item => item.SubItems[e.Column].Text)
                : items.OrderByDescending(item => item.SubItems[e.Column].Text);

            // Clear and re-add sorted items
            listViewAttributes.Items.Clear();
            listViewAttributes.Items.AddRange(sorted.ToArray());

            // Re-enable ItemChecked event
            listViewAttributes.ItemChecked += ListViewAttributes_ItemChecked;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
            SaveCache();
        }
    }
}
