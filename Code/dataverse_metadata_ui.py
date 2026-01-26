"""
Dataverse Metadata Extractor UI
A standalone GUI application for extracting metadata from Dataverse to Power BI semantic model format.

Features:
- Connect to Dataverse using interactive authentication
- Select unmanaged solutions
- Select tables from solution
- Select forms and pre-select attributes from those forms
- Adjust attribute selection
- Save settings for next session
- Export to JSON format
"""

import tkinter as tk
from tkinter import ttk, messagebox, filedialog
import json
import os
import sys
import threading
from concurrent.futures import ThreadPoolExecutor, as_completed
from typing import Dict, List, Set, Optional
from dataclasses import dataclass, field, asdict
from pathlib import Path
import xml.etree.ElementTree as ET

# Add parent directory to path for imports
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))


@dataclass
class AppSettings:
    """Application settings that persist between sessions"""
    environment_url: str = ""
    last_solution: str = ""
    selected_tables: List[str] = field(default_factory=list)
    table_forms: Dict[str, str] = field(default_factory=dict)  # table -> selected form id
    table_views: Dict[str, str] = field(default_factory=dict)  # table -> selected view id
    table_attributes: Dict[str, List[str]] = field(default_factory=dict)  # table -> selected attributes
    output_folder: str = ""
    project_name: str = ""
    window_geometry: str = ""
    
    @classmethod
    def load(cls, path: str) -> 'AppSettings':
        """Load settings from JSON file"""
        try:
            if os.path.exists(path):
                with open(path, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                    return cls(**data)
        except Exception as e:
            print(f"Warning: Could not load settings: {e}")
        return cls()
    
    def save(self, path: str):
        """Save settings to JSON file"""
        try:
            os.makedirs(os.path.dirname(path), exist_ok=True)
            with open(path, 'w', encoding='utf-8') as f:
                json.dump(asdict(self), f, indent=2)
        except Exception as e:
            print(f"Warning: Could not save settings: {e}")


@dataclass 
class MetadataCache:
    """Cached metadata to avoid re-fetching from server"""
    environment_url: str = ""
    solution_name: str = ""
    tables: List[Dict] = field(default_factory=list)  # All solution tables
    table_data: Dict[str, Dict] = field(default_factory=dict)  # logical_name -> table metadata
    table_forms: Dict[str, List[Dict]] = field(default_factory=dict)  # logical_name -> forms list
    table_views: Dict[str, List[Dict]] = field(default_factory=dict)  # logical_name -> views list  
    table_attributes: Dict[str, List[Dict]] = field(default_factory=dict)  # logical_name -> attributes list
    
    @classmethod
    def load(cls, path: str) -> 'MetadataCache':
        """Load cache from JSON file"""
        try:
            if os.path.exists(path):
                with open(path, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                    return cls(**data)
        except Exception as e:
            print(f"Warning: Could not load metadata cache: {e}")
        return cls()
    
    def save(self, path: str):
        """Save cache to JSON file"""
        try:
            os.makedirs(os.path.dirname(path), exist_ok=True)
            with open(path, 'w', encoding='utf-8') as f:
                json.dump(asdict(self), f, indent=2)
        except Exception as e:
            print(f"Warning: Could not save metadata cache: {e}")
    
    def is_valid_for(self, environment_url: str, solution_name: str) -> bool:
        """Check if cache is valid for given environment and solution"""
        return (self.environment_url == environment_url and 
                self.solution_name == solution_name and
                len(self.tables) > 0)


class DataverseClient:
    """Client for Dataverse Web API operations"""
    
    def __init__(self, environment_url: str, access_token: str):
        self.base_url = environment_url.rstrip('/')
        self.api_url = f"{self.base_url}/api/data/v9.2"
        self.access_token = access_token
        self.headers = {
            "Authorization": f"Bearer {access_token}",
            "OData-MaxVersion": "4.0",
            "OData-Version": "4.0",
            "Accept": "application/json",
            "Content-Type": "application/json; charset=utf-8",
            "Prefer": "odata.include-annotations=*"
        }
    
    def _get(self, url: str) -> dict:
        """Make a GET request"""
        import requests
        response = requests.get(url, headers=self.headers, timeout=30)
        response.raise_for_status()
        return response.json()
    
    def get_unmanaged_solutions(self) -> List[Dict]:
        """Get all unmanaged, visible solutions"""
        query = (
            f"{self.api_url}/solutions?"
            f"$select=solutionid,uniquename,friendlyname,version,ismanaged"
            f"&$filter=isvisible eq true and ismanaged eq false"
            f"&$orderby=friendlyname"
        )
        result = self._get(query)
        return result.get('value', [])
    
    def get_solution_tables(self, solution_name: str) -> List[Dict]:
        """Get all tables in a solution - optimized with batch fetching"""
        # First get solution ID
        query = f"{self.api_url}/solutions?$filter=uniquename eq '{solution_name}'&$select=solutionid"
        result = self._get(query)
        solutions = result.get('value', [])
        
        if not solutions:
            raise ValueError(f"Solution '{solution_name}' not found")
        
        solution_id = solutions[0]['solutionid']
        
        # Get solution components (entities = ComponentType 1)
        query = (
            f"{self.api_url}/solutioncomponents?"
            f"$filter=_solutionid_value eq {solution_id} and componenttype eq 1"
            f"&$select=objectid"
        )
        result = self._get(query)
        components = result.get('value', [])
        
        if not components:
            return []
        
        # Build a filter for all entity IDs at once (batch fetch)
        entity_ids = [comp['objectid'] for comp in components]
        
        # Fetch all entities in one request using $filter with 'in' operator
        # Dataverse supports up to ~100 items in an 'in' filter, so batch if needed
        tables = []
        batch_size = 50
        
        for i in range(0, len(entity_ids), batch_size):
            batch_ids = entity_ids[i:i + batch_size]
            id_filter = " or ".join([f"MetadataId eq {eid}" for eid in batch_ids])
            
            query = (
                f"{self.api_url}/EntityDefinitions?"
                f"$filter=({id_filter})"
                f"&$select=LogicalName,SchemaName,DisplayName,ObjectTypeCode,PrimaryIdAttribute,PrimaryNameAttribute,IsActivity,IsIntersect,MetadataId"
            )
            
            try:
                result = self._get(query)
                entities = result.get('value', [])
                
                for entity in entities:
                    # Skip system tables
                    if entity.get('IsActivity') or entity.get('IsIntersect'):
                        continue
                    
                    logical_name = entity.get('LogicalName', '')
                    display_name = entity.get('DisplayName', {}).get('UserLocalizedLabel', {})
                    display_label = display_name.get('Label') if display_name else logical_name
                    
                    tables.append({
                        'LogicalName': logical_name,
                        'DisplayName': display_label,
                        'SchemaName': entity.get('SchemaName'),
                        'ObjectTypeCode': entity.get('ObjectTypeCode'),
                        'PrimaryIdAttribute': entity.get('PrimaryIdAttribute'),
                        'PrimaryNameAttribute': entity.get('PrimaryNameAttribute'),
                        'MetadataId': entity.get('MetadataId')
                    })
            except Exception as e:
                print(f"Warning: Could not get batch entity metadata: {e}")
        
        return sorted(tables, key=lambda x: x.get('DisplayName', ''))
    
    def get_entity_forms(self, entity_logical_name: str, include_xml: bool = False) -> List[Dict]:
        """Get main forms for an entity. Only fetch formxml when needed (include_xml=True)"""
        select_fields = "formid,name"
        if include_xml:
            select_fields += ",formxml"
        
        query = (
            f"{self.api_url}/systemforms?"
            f"$filter=objecttypecode eq '{entity_logical_name}' and type eq 2"
            f"&$select={select_fields}"
            f"&$orderby=name"
        )
        result = self._get(query)
        return result.get('value', [])
    
    def get_form_xml(self, form_id: str) -> str:
        """Get form XML for a specific form - only when needed"""
        query = f"{self.api_url}/systemforms({form_id})?$select=formxml"
        result = self._get(query)
        return result.get('formxml', '')
    
    def get_entity_views(self, entity_logical_name: str, include_fetchxml: bool = False) -> List[Dict]:
        """Get saved queries (views) for an entity. Only fetch fetchxml when needed (include_fetchxml=True)"""
        select_fields = "savedqueryid,name,isdefault,querytype"
        if include_fetchxml:
            select_fields += ",fetchxml"
        
        query = (
            f"{self.api_url}/savedqueries?"
            f"$filter=returnedtypecode eq '{entity_logical_name}' and statecode eq 0"
            f"&$select={select_fields}"
            f"&$orderby=name"
        )
        result = self._get(query)
        views = result.get('value', [])
        # Filter to public views (querytype 0 = public view)
        return [v for v in views if v.get('querytype') == 0]
    
    def get_view_fetchxml(self, view_id: str) -> str:
        """Get FetchXML for a specific view - only when needed"""
        query = f"{self.api_url}/savedqueries({view_id})?$select=fetchxml"
        result = self._get(query)
        return result.get('fetchxml', '')
    
    def get_entity_attributes(self, entity_logical_name: str) -> List[Dict]:
        """Get all attributes for an entity"""
        query = (
            f"{self.api_url}/EntityDefinitions(LogicalName='{entity_logical_name}')/Attributes?"
            f"$select=LogicalName,SchemaName,DisplayName,AttributeType,IsValidForRead,IsCustomAttribute,RequiredLevel"
        )
        result = self._get(query)
        attributes = result.get('value', [])
        
        # Filter and format
        formatted = []
        for attr in attributes:
            if attr.get('IsValidForRead'):
                display_name = attr.get('DisplayName', {}).get('UserLocalizedLabel', {})
                display_label = display_name.get('Label') if display_name else attr.get('SchemaName')
                
                formatted.append({
                    'LogicalName': attr.get('LogicalName'),
                    'SchemaName': attr.get('SchemaName'),
                    'DisplayName': display_label,
                    'AttributeType': attr.get('AttributeType'),
                    'IsCustom': attr.get('IsCustomAttribute', False),
                    'RequiredLevel': attr.get('RequiredLevel', {}).get('Value', 'None')
                })
        
        return sorted(formatted, key=lambda x: x.get('DisplayName', '') or '')
    
    @staticmethod
    def extract_fields_from_form_xml(form_xml: str) -> Set[str]:
        """Extract field logical names from form XML"""
        fields = set()
        try:
            root = ET.fromstring(form_xml)
            for control in root.findall(".//control[@datafieldname]"):
                field_name = control.get('datafieldname')
                if field_name:
                    fields.add(field_name.lower())
        except Exception as e:
            print(f"Warning: Could not parse form XML: {e}")
        return fields


def authenticate_to_dataverse(environment_url: str) -> str:
    """Authenticate to Dataverse using MSAL interactive flow"""
    try:
        from msal import PublicClientApplication
    except ImportError:
        raise ImportError("MSAL not installed. Run: pip install msal")
    
    import re
    match = re.search(r'https://([^/]+)', environment_url)
    if not match:
        raise ValueError(f"Invalid environment URL: {environment_url}")
    
    resource_url = f"https://{match.group(1)}"
    scopes = [f"{resource_url}/.default"]
    
    client_id = "51f81489-12ee-4a9e-aaae-a2591f45987d"
    authority = "https://login.microsoftonline.com/organizations"
    
    app = PublicClientApplication(client_id=client_id, authority=authority)
    
    # Try cached token first
    accounts = app.get_accounts()
    if accounts:
        result = app.acquire_token_silent(scopes, account=accounts[0])
        if result and 'access_token' in result:
            return result['access_token']
    
    # Interactive login
    result = app.acquire_token_interactive(scopes=scopes)
    
    if 'access_token' in result:
        return result['access_token']
    else:
        error = result.get('error', 'Unknown error')
        error_desc = result.get('error_description', 'No description')
        raise Exception(f"Authentication failed: {error} - {error_desc}")


class DataverseMetadataApp:
    """Main application window"""
    
    SETTINGS_FILE = os.path.join(os.path.dirname(__file__), '.dataverse_metadata_settings.json')
    CACHE_FILE = os.path.join(os.path.dirname(__file__), '.dataverse_metadata_cache.json')
    
    def __init__(self):
        self.root = tk.Tk()
        self.root.title("Dataverse Metadata Extractor for Power BI")
        self.root.geometry("1200x800")
        self.root.minsize(1000, 600)
        
        # State
        self.settings = AppSettings.load(self.SETTINGS_FILE)
        self.cache = MetadataCache.load(self.CACHE_FILE)
        self.client: Optional[DataverseClient] = None
        self.solutions: List[Dict] = []
        self.solution_values: List[str] = []
        self.tables: List[Dict] = []
        self.selected_tables: Dict[str, Dict] = {}  # logical_name -> table data
        self.table_forms: Dict[str, List[Dict]] = {}  # logical_name -> forms
        self.table_views: Dict[str, List[Dict]] = {}  # logical_name -> views
        self.table_attributes: Dict[str, List[Dict]] = {}  # logical_name -> attributes
        self.selected_attributes: Dict[str, Set[str]] = {}  # logical_name -> selected attribute names
        self.selected_views: Dict[str, str] = {}  # logical_name -> selected view id
        self.table_load_state: Dict[str, Dict[str, bool]] = {}  # logical_name -> load flags
        
        # Sorting state for treeviews
        self.sort_state: Dict[str, tuple] = {}  # tree_id -> (column, reverse)
        
        # Attribute filter mode: 'all' or 'selected'
        self.attr_show_mode = tk.StringVar(value='all')
        
        # Restore window geometry
        if self.settings.window_geometry:
            try:
                self.root.geometry(self.settings.window_geometry)
            except:
                pass
        
        self._create_styles()
        self._create_ui()
        self._restore_settings()
        self._restore_from_cache()
        
        # Handle window close
        self.root.protocol("WM_DELETE_WINDOW", self._on_close)
    
    def _create_styles(self):
        """Configure ttk styles"""
        style = ttk.Style()
        style.theme_use('clam')
        
        # Configure colors
        style.configure("TFrame", background="#f0f0f0")
        style.configure("TLabel", background="#f0f0f0", font=('Segoe UI', 10))
        style.configure("TButton", font=('Segoe UI', 10), padding=6)
        style.configure("Header.TLabel", font=('Segoe UI', 12, 'bold'))
        style.configure("Status.TLabel", font=('Segoe UI', 9))
        style.configure("Treeview", font=('Segoe UI', 10), rowheight=25)
        style.configure("Treeview.Heading", font=('Segoe UI', 10, 'bold'))
    
    def _create_ui(self):
        """Create the main UI"""
        # Main container
        main_frame = ttk.Frame(self.root, padding="10")
        main_frame.pack(fill=tk.BOTH, expand=True)
        
        # --- Connection Frame ---
        conn_frame = ttk.LabelFrame(main_frame, text="Connection", padding="10")
        conn_frame.pack(fill=tk.X, pady=(0, 10))
        
        ttk.Label(conn_frame, text="Dataverse URL:").grid(row=0, column=0, sticky=tk.W, padx=(0, 10))
        self.url_var = tk.StringVar(value=self.settings.environment_url)
        self.url_entry = ttk.Entry(conn_frame, textvariable=self.url_var, width=50)
        self.url_entry.grid(row=0, column=1, sticky=tk.EW, padx=(0, 10))
        
        self.connect_btn = ttk.Button(conn_frame, text="Connect", command=self._on_connect)
        self.connect_btn.grid(row=0, column=2)
        
        self.conn_status = ttk.Label(conn_frame, text="Not connected", style="Status.TLabel")
        self.conn_status.grid(row=0, column=3, padx=(10, 0))
        
        conn_frame.columnconfigure(1, weight=1)

        # --- Table Selector Button ---
        table_selector_frame = ttk.Frame(main_frame)
        table_selector_frame.pack(fill=tk.X, pady=(0, 10))

        self.open_table_selector_btn = ttk.Button(
            table_selector_frame,
            text="Select Tables...",
            command=self._open_table_selector
        )
        self.open_table_selector_btn.pack(side=tk.LEFT)

        self.table_count_label = ttk.Label(table_selector_frame, text="No tables selected", style="Status.TLabel")
        self.table_count_label.pack(side=tk.LEFT, padx=(10, 0))
        
        # --- Main Content Paned Window ---
        paned = ttk.PanedWindow(main_frame, orient=tk.HORIZONTAL)
        paned.pack(fill=tk.BOTH, expand=True, pady=(0, 10))
        
        # Initialize solution/table selector controls (created in modal)
        self.solution_var = tk.StringVar()
        self.solution_combo = None
        self.load_tables_btn = None
        self.tables_tree = None
        
        # Middle panel - Selected Tables with Forms
        middle_frame = ttk.Frame(paned)
        paned.add(middle_frame, weight=1)
        
        sel_tables_frame = ttk.LabelFrame(middle_frame, text="Selected Tables & Forms", padding="5")
        sel_tables_frame.pack(fill=tk.BOTH, expand=True)
        
        self.selected_tree = ttk.Treeview(sel_tables_frame, columns=('display', 'form', 'filter', 'attrs', 'edit'), show='headings', selectmode='browse')
        self.selected_tree.heading('display', text='Table', command=lambda: self._sort_treeview(self.selected_tree, 'display'))
        self.selected_tree.heading('form', text='Form', command=lambda: self._sort_treeview(self.selected_tree, 'form'))
        self.selected_tree.heading('filter', text='Filter', command=lambda: self._sort_treeview(self.selected_tree, 'filter'))
        self.selected_tree.heading('attrs', text='Attrs', command=lambda: self._sort_treeview(self.selected_tree, 'attrs'))
        self.selected_tree.heading('edit', text='Edit')
        self.selected_tree.column('display', width=150)
        self.selected_tree.column('form', width=160)
        self.selected_tree.column('filter', width=160)
        self.selected_tree.column('attrs', width=50)
        self.selected_tree.column('edit', width=60, anchor='center')
        
        sel_scroll = ttk.Scrollbar(sel_tables_frame, orient=tk.VERTICAL, command=self.selected_tree.yview)
        self.selected_tree.configure(yscrollcommand=sel_scroll.set)
        
        self.selected_tree.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        sel_scroll.pack(side=tk.RIGHT, fill=tk.Y)
        
        self.selected_tree.bind('<<TreeviewSelect>>', self._on_selected_table_click)
        self.selected_tree.bind('<Button-1>', self._on_table_tree_click)

        # Remove table button
        remove_frame = ttk.Frame(middle_frame)
        remove_frame.pack(fill=tk.X, pady=(5, 0))
        self.remove_table_btn = ttk.Button(remove_frame, text="Remove Selected Table", command=self._on_remove_tables)
        self.remove_table_btn.pack(side=tk.LEFT)
        
        # Hidden combo boxes for form/view selection (shown on demand)
        self.form_var = tk.StringVar()
        self.view_var = tk.StringVar()
        self.editing_combo = None  # Will hold the popup combo when editing
        
        # Right panel - Attributes
        right_frame = ttk.Frame(paned)
        paned.add(right_frame, weight=1)
        
        attr_frame = ttk.LabelFrame(right_frame, text="Attributes", padding="5")
        attr_frame.pack(fill=tk.BOTH, expand=True)
        
        # Attribute filter
        filter_frame = ttk.Frame(attr_frame)
        filter_frame.pack(fill=tk.X, pady=(0, 5))
        
        ttk.Label(filter_frame, text="Search:").pack(side=tk.LEFT, padx=(0, 5))
        self.attr_filter_var = tk.StringVar()
        self.attr_filter_var.trace('w', self._on_attr_filter_changed)
        ttk.Entry(filter_frame, textvariable=self.attr_filter_var, width=15).pack(side=tk.LEFT, padx=(0, 10))
        
        ttk.Label(filter_frame, text="Show:").pack(side=tk.LEFT, padx=(0, 5))
        ttk.Radiobutton(filter_frame, text="All", variable=self.attr_show_mode, value='all', 
                        command=self._on_attr_filter_changed).pack(side=tk.LEFT, padx=(0, 5))
        ttk.Radiobutton(filter_frame, text="Selected", variable=self.attr_show_mode, value='selected',
                        command=self._on_attr_filter_changed).pack(side=tk.LEFT)
        
        # Attributes list with checkboxes
        self.attr_tree = ttk.Treeview(attr_frame, columns=('selected', 'on_form', 'display', 'logical', 'type'), show='headings', selectmode='extended')
        self.attr_tree.heading('selected', text='Selected', command=lambda: self._sort_treeview(self.attr_tree, 'selected'))
        self.attr_tree.heading('on_form', text='On Form', command=lambda: self._sort_treeview(self.attr_tree, 'on_form'))
        self.attr_tree.heading('display', text='Display Name', command=lambda: self._sort_treeview(self.attr_tree, 'display'))
        self.attr_tree.heading('logical', text='Logical Name', command=lambda: self._sort_treeview(self.attr_tree, 'logical'))
        self.attr_tree.heading('type', text='Type', command=lambda: self._sort_treeview(self.attr_tree, 'type'))
        self.attr_tree.column('selected', width=60, anchor='center')
        self.attr_tree.column('on_form', width=60, anchor='center')
        self.attr_tree.column('display', width=140)
        self.attr_tree.column('logical', width=110)
        self.attr_tree.column('type', width=90)
        
        # Configure tag colors once at startup
        self.attr_tree.tag_configure('required', foreground='#0066cc', font=('Segoe UI', 10, 'bold'))
        self.attr_tree.tag_configure('selected', foreground='black', font=('Segoe UI', 10))
        self.attr_tree.tag_configure('unselected', foreground='gray', font=('Segoe UI', 10))
        
        attr_scroll = ttk.Scrollbar(attr_frame, orient=tk.VERTICAL, command=self.attr_tree.yview)
        self.attr_tree.configure(yscrollcommand=attr_scroll.set)
        
        self.attr_tree.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        attr_scroll.pack(side=tk.RIGHT, fill=tk.Y)
        
        self.attr_tree.bind('<Double-1>', self._on_attr_toggle)
        self.attr_tree.bind('<space>', self._on_attr_toggle)
        self.attr_tree.bind('<Button-1>', self._on_attr_click)
        
        # Attribute buttons
        attr_btn_frame = ttk.Frame(right_frame)
        attr_btn_frame.pack(fill=tk.X, pady=(5, 0))
        
        ttk.Button(attr_btn_frame, text="Select All", command=self._on_select_all_attrs).pack(side=tk.LEFT, padx=(0, 5))
        ttk.Button(attr_btn_frame, text="Deselect All", command=self._on_deselect_all_attrs).pack(side=tk.LEFT, padx=(0, 5))
        ttk.Button(attr_btn_frame, text="Select From Form", command=self._on_select_form_attrs).pack(side=tk.LEFT)
        
        # --- Output Frame ---
        output_frame = ttk.LabelFrame(main_frame, text="Output", padding="10")
        output_frame.pack(fill=tk.X, pady=(0, 10))
        
        ttk.Label(output_frame, text="Project Name:").grid(row=0, column=0, sticky=tk.W, padx=(0, 10))
        self.project_var = tk.StringVar(value=self.settings.project_name)
        ttk.Entry(output_frame, textvariable=self.project_var, width=30).grid(row=0, column=1, sticky=tk.W, padx=(0, 20))
        
        ttk.Label(output_frame, text="Output Folder:").grid(row=0, column=2, sticky=tk.W, padx=(0, 10))
        self.output_var = tk.StringVar(value=self.settings.output_folder)
        ttk.Entry(output_frame, textvariable=self.output_var, width=50).grid(row=0, column=3, sticky=tk.EW, padx=(0, 10))
        
        ttk.Button(output_frame, text="Browse...", command=self._on_browse_output).grid(row=0, column=4)
        
        output_frame.columnconfigure(3, weight=1)
        
        # --- Action Buttons ---
        action_frame = ttk.Frame(main_frame)
        action_frame.pack(fill=tk.X)
        
        self.export_btn = ttk.Button(action_frame, text="Export Metadata JSON", command=self._on_export)
        self.export_btn.pack(side=tk.RIGHT)
        
        self.status_label = ttk.Label(action_frame, text="Ready", style="Status.TLabel")
        self.status_label.pack(side=tk.LEFT)
    
    def _restore_settings(self):
        """Restore settings from previous session"""
        if self.settings.environment_url:
            self.url_var.set(self.settings.environment_url)
        if self.settings.project_name:
            self.project_var.set(self.settings.project_name)
        if self.settings.output_folder:
            self.output_var.set(self.settings.output_folder)
        self._update_table_count()
    
    def _restore_from_cache(self):
        """Restore tables and metadata from cache if valid"""
        env_url = self.settings.environment_url
        solution = self.settings.last_solution
        
        # If settings don't have solution, try getting it from cache
        if not solution and self.cache.solution_name:
            solution = self.cache.solution_name
        
        if not env_url or not solution:
            return
        
        if not self.cache.is_valid_for(env_url, solution):
            return
        
        self._set_status("Restoring from cache...")
        
        # Update settings with cache solution if missing
        if not self.settings.last_solution:
            self.settings.last_solution = solution
        
        # Restore tables list
        self.tables = self.cache.tables
        
        # Restore selected tables with their cached metadata
        for logical_name in self.cache.table_data.keys():
            table_data = self.cache.table_data.get(logical_name)
            if not table_data:
                continue
            
            self.selected_tables[logical_name] = table_data
            
            # Restore cached forms, views, attributes
            cached_forms = self.cache.table_forms.get(logical_name, [])
            cached_views = self.cache.table_views.get(logical_name, [])
            cached_attrs = self.cache.table_attributes.get(logical_name, [])
            
            self.table_forms[logical_name] = cached_forms
            self.table_views[logical_name] = cached_views
            self.table_attributes[logical_name] = cached_attrs
            
            # Mark as loaded
            self.table_load_state[logical_name] = {
                'attrs_loaded': len(cached_attrs) > 0,
                'forms_loaded': len(cached_forms) > 0,
                'views_loaded': len(cached_views) > 0,
                'attrs_loading': False,
                'forms_loading': False,
                'views_loading': False
            }
            
            # Restore attribute selections
            saved_attrs = self.settings.table_attributes.get(logical_name, [])
            required_attrs = self._get_required_attributes(logical_name)
            # Always include required attributes
            self.selected_attributes[logical_name] = required_attrs.copy()
            self.selected_attributes[logical_name].update(saved_attrs)
            
            # Restore view selection
            saved_view_id = self.settings.table_views.get(logical_name)
            if saved_view_id:
                self.selected_views[logical_name] = saved_view_id
            elif cached_views:
                default_view = next((v for v in cached_views if v.get('isdefault')), cached_views[0])
                self.selected_views[logical_name] = default_view.get('savedqueryid')
            
            # Determine form/view display names
            form_name = '(no forms)'
            if cached_forms:
                saved_form_id = self.settings.table_forms.get(logical_name)
                sel_form = next((f for f in cached_forms if f.get('formid') == saved_form_id), None) or cached_forms[0]
                form_name = sel_form.get('name', 'Unnamed')
            
            view_name = '(no views)'
            if cached_views:
                sel_view = next((v for v in cached_views if v.get('savedqueryid') == self.selected_views.get(logical_name)), None)
                if sel_view:
                    view_name = sel_view.get('name', 'Unnamed')
            
            # Add to tree
            self.selected_tree.insert('', tk.END, iid=logical_name, values=(
                table_data.get('DisplayName', ''),
                form_name,
                view_name,
                len(self.selected_attributes.get(logical_name, set())),
                '✏️ Edit'
            ))
        
        self._update_table_count()
        
        # Set solution_var for proper autosave
        if self.settings.last_solution:
            self.solution_var.set(self.settings.last_solution)
        
        # Auto-select first table to display attributes
        children = self.selected_tree.get_children()
        if children:
            first_table = children[0]
            self.selected_tree.selection_set(first_table)
            self._update_attributes_display(first_table)
        
        # Save settings to persist the restored solution
        self._autosave()
        
        self._set_status(f"Restored {len(self.selected_tables)} table(s) from cache")
    
    def _save_settings(self):
        """Save current settings"""
        self.settings.environment_url = self.url_var.get()
        self.settings.project_name = self.project_var.get()
        self.settings.output_folder = self.output_var.get()
        self.settings.last_solution = self.solution_var.get().split(' - ')[0] if self.solution_var.get() else ""
        self.settings.selected_tables = list(self.selected_tables.keys())
        self.settings.table_forms = {k: v for k, v in self._get_selected_forms().items()}
        self.settings.table_views = {k: v for k, v in self.selected_views.items()}
        self.settings.table_attributes = {k: list(v) for k, v in self.selected_attributes.items()}
        self.settings.window_geometry = self.root.geometry()
        self.settings.save(self.SETTINGS_FILE)

    def _autosave(self):
        """Persist settings on change"""
        self._save_settings()
    
    def _get_required_attributes(self, logical_name: str) -> Set[str]:
        """Get the required attributes (primary ID and name) for an entity"""
        required = set()
        table_data = self.selected_tables.get(logical_name)
        if table_data:
            primary_id = table_data.get('PrimaryIdAttribute')
            primary_name = table_data.get('PrimaryNameAttribute')
            if primary_id:
                required.add(primary_id)
            if primary_name:
                required.add(primary_name)
        return required
    
    def _get_selected_forms(self) -> Dict[str, str]:
        """Get currently selected form IDs per table"""
        # Returns table_logical_name -> form_id mapping
        result = {}
        for logical_name in self.selected_tree.get_children():
            values = self.selected_tree.item(logical_name, 'values')
            if len(values) >= 2:
                selected_form_name = values[1]  # Form column
                # Find form ID for this table's selected form name
                if selected_form_name and selected_form_name not in ['(not loaded)', '(loading...)', '(no forms)']:
                    for form in self.table_forms.get(logical_name, []):
                        if form.get('name') == selected_form_name:
                            result[logical_name] = form.get('formid')
                            break
        return result
        return result
    
    def _set_status(self, message: str):
        """Update status bar"""
        self.status_label.config(text=message)
        self.root.update_idletasks()

    def _update_table_count(self):
        """Update selected table count label"""
        count = len(self.selected_tables)
        if count == 0:
            text = "No tables selected"
        elif count == 1:
            text = "1 table selected"
        else:
            text = f"{count} tables selected"
        self.table_count_label.config(text=text)
    
    def _on_connect(self):
        """Handle connect button click"""
        url = self.url_var.get().strip()
        if not url:
            messagebox.showerror("Error", "Please enter a Dataverse URL")
            return
        
        if not url.startswith('https://'):
            url = f"https://{url}"
            self.url_var.set(url)
        
        self.connect_btn.config(state='disabled')
        self.conn_status.config(text="Connecting...")
        self._set_status("Authenticating to Dataverse...")
        
        def do_connect():
            try:
                token = authenticate_to_dataverse(url)
                self.client = DataverseClient(url, token)
                
                # Load solutions
                self.solutions = self.client.get_unmanaged_solutions()
                
                self.root.after(0, lambda: self._on_connect_success())
            except Exception as e:
                self.root.after(0, lambda: self._on_connect_error(str(e)))
        
        threading.Thread(target=do_connect, daemon=True).start()
    
    def _on_connect_success(self):
        """Handle successful connection"""
        self.conn_status.config(text="Connected")
        self.connect_btn.config(state='normal')
        
        # Populate solutions dropdown
        self.solution_values = [f"{s['uniquename']} - {s['friendlyname']}" for s in self.solutions]
        if self.solution_combo is not None:
            self.solution_combo['values'] = self.solution_values
            # Restore last solution if available
            if self.settings.last_solution:
                for i, s in enumerate(self.solutions):
                    if s['uniquename'] == self.settings.last_solution:
                        self.solution_combo.current(i)
                        break
        
        self._set_status(f"Connected. Found {len(self.solutions)} solution(s).")

        # Auto-load tables if we don't have cached data or need to refresh
        if self.settings.last_solution and not self.selected_tables:
            self.solution_var.set(self.settings.last_solution)
            self._load_tables_for_solution(self.settings.last_solution, auto_select=True)

    def _open_table_selector(self):
        """Open modal dialog for selecting solution and tables"""
        if not self.client:
            messagebox.showerror("Error", "Please connect first")
            return

        dialog = tk.Toplevel(self.root)
        dialog.title("Select Tables")
        dialog.geometry("900x600")
        dialog.transient(self.root)
        dialog.grab_set()

        container = ttk.Frame(dialog, padding="10")
        container.pack(fill=tk.BOTH, expand=True)

        # Solutions
        sol_frame = ttk.LabelFrame(container, text="Solutions", padding="5")
        sol_frame.pack(fill=tk.X, pady=(0, 10))

        self.solution_combo = ttk.Combobox(sol_frame, textvariable=self.solution_var, state='readonly', width=50)
        self.solution_combo.pack(fill=tk.X, side=tk.LEFT, expand=True)
        self.solution_combo.bind('<<ComboboxSelected>>', self._on_solution_selected)

        self.load_tables_btn = ttk.Button(sol_frame, text="Load Tables", command=self._on_load_tables)
        self.load_tables_btn.pack(side=tk.RIGHT, padx=(10, 0))

        if self.solution_values:
            self.solution_combo['values'] = self.solution_values
        
        # Restore last solution if available
        if self.settings.last_solution:
            for i, s in enumerate(self.solutions):
                if s['uniquename'] == self.settings.last_solution:
                    self.solution_combo.current(i)
                    break

        # Available Tables
        avail_frame = ttk.LabelFrame(container, text="Available Tables in Solution", padding="5")
        avail_frame.pack(fill=tk.BOTH, expand=True, pady=(0, 10))

        self.tables_tree = ttk.Treeview(avail_frame, columns=('display', 'logical'), show='headings', selectmode='extended')
        self.tables_tree.heading('display', text='Display Name', command=lambda: self._sort_treeview(self.tables_tree, 'display'))
        self.tables_tree.heading('logical', text='Logical Name', command=lambda: self._sort_treeview(self.tables_tree, 'logical'))
        self.tables_tree.column('display', width=350)
        self.tables_tree.column('logical', width=250)

        tables_scroll = ttk.Scrollbar(avail_frame, orient=tk.VERTICAL, command=self.tables_tree.yview)
        self.tables_tree.configure(yscrollcommand=tables_scroll.set)

        self.tables_tree.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        tables_scroll.pack(side=tk.RIGHT, fill=tk.Y)

        # Buttons
        btn_frame = ttk.Frame(container)
        btn_frame.pack(fill=tk.X)

        self.add_table_btn = ttk.Button(btn_frame, text="Add Selected", command=self._on_add_tables)
        self.add_table_btn.pack(side=tk.LEFT)

        ttk.Button(btn_frame, text="Close", command=dialog.destroy).pack(side=tk.RIGHT)
    
    def _on_connect_error(self, error: str):
        """Handle connection error"""
        self.conn_status.config(text="Connection failed")
        self.connect_btn.config(state='normal')
        self._set_status("Connection failed")
        messagebox.showerror("Connection Error", f"Failed to connect:\n{error}")
    
    def _on_solution_selected(self, event=None):
        """Handle solution selection"""
        pass  # Just wait for Load Tables button
    
    def _on_load_tables(self):
        """Load tables from selected solution"""
        if not self.client:
            messagebox.showerror("Error", "Please connect first")
            return
        
        selection = self.solution_var.get()
        if not selection:
            messagebox.showerror("Error", "Please select a solution")
            return
        
        solution_name = selection.split(' - ')[0]
        self._load_tables_for_solution(solution_name, auto_select=True)

    def _load_tables_for_solution(self, solution_name: str, auto_select: bool = False):
        """Load tables for a solution and optionally auto-select saved tables"""
        if self.load_tables_btn is not None:
            self.load_tables_btn.config(state='disabled')
        self._set_status(f"Loading tables from {solution_name}...")

        def do_load():
            try:
                self.tables = self.client.get_solution_tables(solution_name)
                self.root.after(0, lambda: self._on_tables_loaded(auto_select=auto_select))
            except Exception as e:
                self.root.after(0, lambda: self._on_tables_error(str(e)))

        threading.Thread(target=do_load, daemon=True).start()
    
    def _on_tables_loaded(self, auto_select: bool = False):
        """Handle tables loaded"""
        if self.load_tables_btn is not None:
            self.load_tables_btn.config(state='normal')
        
        # Clear and populate tables tree
        if self.tables_tree is not None:
            for item in self.tables_tree.get_children():
                self.tables_tree.delete(item)
            
            for table in self.tables:
                self.tables_tree.insert('', tk.END, values=(
                    table.get('DisplayName', ''),
                    table.get('LogicalName', '')
                ))
        
        self._set_status(f"Loaded {len(self.tables)} table(s)")
        
        # Restore previously selected tables if any
        if auto_select and self.settings.selected_tables:
            tables_to_add = []
            for logical_name in self.settings.selected_tables:
                for table in self.tables:
                    if table.get('LogicalName') == logical_name:
                        tables_to_add.append(table)
                        break
            
            if tables_to_add:
                self._add_tables_bulk(tables_to_add)
        
        self._update_table_count()
    
    def _add_tables_bulk(self, tables: List[Dict]):
        """Add multiple tables and load their metadata in parallel"""
        # First add all tables to the tree
        for table in tables:
            logical_name = table.get('LogicalName')
            if logical_name in self.selected_tables:
                continue  # Already added
            
            self.selected_tables[logical_name] = table
            self.selected_attributes[logical_name] = set()
            self.table_load_state[logical_name] = {
                'attrs_loaded': False,
                'forms_loaded': False,
                'views_loaded': False,
                'attrs_loading': True,
                'forms_loading': False,
                'views_loading': False
            }
            
            # Add to tree
            self.selected_tree.insert('', tk.END, iid=logical_name, values=(
                table.get('DisplayName', ''),
                '(loading...)',
                '(loading...)',
                '0'
            ))
        
        # Now load attributes for all tables in parallel
        self._set_status(f"Loading attributes for {len(tables)} tables...")
        
        def do_parallel_load():
            results = {}
            with ThreadPoolExecutor(max_workers=5) as executor:
                futures = {
                    executor.submit(self.client.get_entity_attributes, t.get('LogicalName')): t.get('LogicalName')
                    for t in tables
                }
                
                for future in as_completed(futures):
                    logical_name = futures[future]
                    try:
                        attributes = future.result()
                        results[logical_name] = attributes
                    except Exception as e:
                        print(f"Error loading attributes for {logical_name}: {e}")
                        results[logical_name] = []
            
            self.root.after(0, lambda: self._on_bulk_attributes_loaded(results))
        
        threading.Thread(target=do_parallel_load, daemon=True).start()
    
    def _on_bulk_attributes_loaded(self, results: Dict[str, List[Dict]]):
        """Handle bulk attribute loading completion"""
        for logical_name, attributes in results.items():
            if logical_name not in self.selected_tables:
                continue
            
            self.table_attributes[logical_name] = attributes
            state = self.table_load_state.get(logical_name)
            if state:
                state['attrs_loaded'] = True
                state['attrs_loading'] = False
            
            # Always add required attributes (primary ID and name)
            required_attrs = self._get_required_attributes(logical_name)
            self.selected_attributes[logical_name] = required_attrs.copy()
            
            # Restore saved attribute selections if available
            saved_attrs = self.settings.table_attributes.get(logical_name)
            if saved_attrs:
                self.selected_attributes[logical_name].update(saved_attrs)
            else:
                # Add standard important fields by default
                important_fields = {'createdon', 'modifiedon', 'createdby', 'modifiedby', 'ownerid', 'statecode', 'statuscode'}
                for attr in attributes:
                    if attr.get('LogicalName', '').lower() in important_fields:
                        self.selected_attributes[logical_name].add(attr.get('LogicalName'))
            
            # Update count in tree
            if logical_name in self.selected_tables:
                values = list(self.selected_tree.item(logical_name, 'values'))
                values[3] = len(self.selected_attributes[logical_name])
                self.selected_tree.item(logical_name, values=values)
        
        # Update display if a table is selected
        selection = self.selected_tree.selection()
        if selection and selection[0] in results:
            self._update_attributes_display(selection[0])
        
        self._set_status(f"Loaded attributes for {len(results)} tables")
        
        # Now load forms and views in parallel for all tables
        self._load_forms_views_bulk(list(results.keys()))
        self._autosave()
    
    def _on_tables_error(self, error: str):
        """Handle tables load error"""
        if self.load_tables_btn is not None:
            self.load_tables_btn.config(state='normal')
        self._set_status("Failed to load tables")
        messagebox.showerror("Error", f"Failed to load tables:\n{error}")
    
    def _load_forms_views_bulk(self, logical_names: List[str]):
        """Load forms and views for multiple tables in parallel"""
        tables_to_load = []
        for logical_name in logical_names:
            state = self.table_load_state.get(logical_name, {})
            if not state.get('forms_loaded') and not state.get('forms_loading'):
                tables_to_load.append(logical_name)
                if state:
                    state['forms_loading'] = True
                    state['views_loading'] = True
        
        if not tables_to_load:
            return
        
        self._set_status(f"Loading forms/views for {len(tables_to_load)} tables...")
        
        def do_parallel_load():
            results = {}
            with ThreadPoolExecutor(max_workers=5) as executor:
                futures = {}
                for logical_name in tables_to_load:
                    futures[executor.submit(self._fetch_forms_views, logical_name)] = logical_name
                
                for future in as_completed(futures):
                    logical_name = futures[future]
                    try:
                        forms, views = future.result()
                        results[logical_name] = (forms, views)
                    except Exception as e:
                        print(f"Error loading forms/views for {logical_name}: {e}")
                        results[logical_name] = ([], [])
            
            self.root.after(0, lambda: self._on_bulk_forms_views_loaded(results))
        
        threading.Thread(target=do_parallel_load, daemon=True).start()
    
    def _fetch_forms_views(self, logical_name: str):
        """Fetch forms and views for a single table (called from thread)"""
        forms = self.client.get_entity_forms(logical_name, include_xml=True)
        views = self.client.get_entity_views(logical_name)
        return forms, views
    
    def _on_bulk_forms_views_loaded(self, results: Dict[str, tuple]):
        """Handle bulk forms/views loading completion"""
        for logical_name, (forms, views) in results.items():
            if logical_name not in self.selected_tables:
                continue
            
            self.table_forms[logical_name] = forms
            self.table_views[logical_name] = views
            
            state = self.table_load_state.get(logical_name)
            if state:
                state['forms_loaded'] = True
                state['views_loaded'] = True
                state['forms_loading'] = False
                state['views_loading'] = False
            
            # Determine form/view names
            form_name = '(no forms)'
            if forms:
                saved_form_id = self.settings.table_forms.get(logical_name)
                sel_form = next((f for f in forms if f.get('formid') == saved_form_id), None) or forms[0]
                form_name = sel_form.get('name', 'Unnamed')
            
            view_name = '(no views)'
            if views:
                saved_view_id = self.settings.table_views.get(logical_name)
                sel_view = next((v for v in views if v.get('savedqueryid') == saved_view_id), None)
                if not sel_view:
                    sel_view = next((v for v in views if v.get('isdefault')), views[0])
                view_name = sel_view.get('name', 'Unnamed')
                self.selected_views[logical_name] = sel_view.get('savedqueryid')
            
            # Update tree
            values = list(self.selected_tree.item(logical_name, 'values'))
            values[1] = form_name
            values[2] = view_name
            self.selected_tree.item(logical_name, values=values)
        
        self._set_status(f"Loaded forms/views for {len(results)} tables")
        self._save_cache()
        self._autosave()
        
        # Auto-select first table to show its attributes
        children = self.selected_tree.get_children()
        if children and not self.selected_tree.selection():
            self.selected_tree.selection_set(children[0])
            self._on_selected_table_click()
    
    def _save_cache(self):
        """Save current metadata to cache"""
        env_url = self.url_var.get()
        solution = self.solution_var.get().split(' - ')[0] if self.solution_var.get() else self.settings.last_solution
        
        if not env_url or not solution:
            return
        
        self.cache.environment_url = env_url
        self.cache.solution_name = solution
        self.cache.tables = self.tables
        self.cache.table_data = {k: v for k, v in self.selected_tables.items()}
        self.cache.table_forms = {k: v for k, v in self.table_forms.items()}
        self.cache.table_views = {k: v for k, v in self.table_views.items()}
        self.cache.table_attributes = {k: v for k, v in self.table_attributes.items()}
        self.cache.save(self.CACHE_FILE)
    
    def _on_add_tables(self):
        """Add selected tables to selection"""
        if self.tables_tree is None:
            return
        selection = self.tables_tree.selection()
        if not selection:
            return
        
        tables_to_add = []
        for item_id in selection:
            values = self.tables_tree.item(item_id, 'values')
            logical_name = values[1]
            
            # Skip if already added
            if logical_name in self.selected_tables:
                continue
            
            # Find full table data
            for table in self.tables:
                if table.get('LogicalName') == logical_name:
                    tables_to_add.append(table)
                    break
        
        if tables_to_add:
            if len(tables_to_add) == 1:
                self._add_table_to_selection(tables_to_add[0])
            else:
                self._add_tables_bulk(tables_to_add)
        
        self._update_table_count()
        self._autosave()
    
    def _add_table_to_selection(self, table: Dict):
        """Add a table to the selection and load its forms/attributes"""
        logical_name = table.get('LogicalName')
        if logical_name in self.selected_tables:
            return  # Already added
        
        self.selected_tables[logical_name] = table
        self.selected_attributes[logical_name] = set()
        self.table_load_state[logical_name] = {
            'attrs_loaded': False,
            'forms_loaded': False,
            'views_loaded': False,
            'attrs_loading': False,
            'forms_loading': False,
            'views_loading': False
        }
        
        # Add to tree
        self.selected_tree.insert('', tk.END, iid=logical_name, values=(
            table.get('DisplayName', ''),
            '(not loaded)',
            '(not loaded)',
            '0',
            '✏️ Edit'
        ))
        
        # Load attributes in background (forms/views are lazy-loaded on selection)
        self._load_attributes_for_table(logical_name)
    
    def _load_attributes_for_table(self, logical_name: str):
        """Load attributes for a table in the background"""
        if logical_name not in self.table_load_state:
            return
        if self.table_load_state[logical_name]['attrs_loading']:
            return
        
        self.table_load_state[logical_name]['attrs_loading'] = True
        
        def do_load():
            try:
                attributes = self.client.get_entity_attributes(logical_name)
                self.root.after(0, lambda: self._on_attributes_loaded(logical_name, attributes))
            except Exception as e:
                self.root.after(0, lambda: self._on_attributes_error(logical_name, str(e)))
        
        threading.Thread(target=do_load, daemon=True).start()
    
    def _on_attributes_loaded(self, logical_name: str, attributes: List[Dict]):
        """Handle attributes loaded for single table"""
        if logical_name not in self.selected_tables:
            return
        
        self.table_attributes[logical_name] = attributes
        state = self.table_load_state[logical_name]
        state['attrs_loaded'] = True
        state['attrs_loading'] = False
        
        # Always add required attributes (primary ID and name)
        required_attrs = self._get_required_attributes(logical_name)
        self.selected_attributes[logical_name].update(required_attrs)
        
        # Restore saved attribute selections if available
        saved_attrs = self.settings.table_attributes.get(logical_name)
        if saved_attrs:
            self.selected_attributes[logical_name].update(saved_attrs)
        else:
            # Add standard important fields by default
            important_fields = {'createdon', 'modifiedon', 'createdby', 'modifiedby', 'ownerid', 'statecode', 'statuscode'}
            for attr in attributes:
                if attr.get('LogicalName', '').lower() in important_fields:
                    self.selected_attributes[logical_name].add(attr.get('LogicalName'))
        
        # Update count in selected table tree
        if logical_name in self.selected_tables:
            values = list(self.selected_tree.item(logical_name, 'values'))
            values[3] = len(self.selected_attributes[logical_name])
            self.selected_tree.item(logical_name, values=values)
        
        # If this table is selected, refresh attribute display
        selection = self.selected_tree.selection()
        if selection and selection[0] == logical_name:
            self._update_attributes_display(logical_name)
        elif not selection:
            # Auto-select first table if nothing selected yet
            children = self.selected_tree.get_children()
            if children:
                self.selected_tree.selection_set(children[0])
                self._on_selected_table_click()
        
        # Also load forms/views for this table
        self._load_forms_views_bulk([logical_name])
        self._autosave()
    
    def _on_attributes_error(self, logical_name: str, error: str):
        """Handle attribute load error"""
        state = self.table_load_state.get(logical_name)
        if state:
            state['attrs_loading'] = False
        print(f"Error loading attributes for {logical_name}: {error}")
    
    def _load_forms_views_for_table(self, logical_name: str):
        """Lazy-load forms and views for a table"""
        if logical_name not in self.table_load_state:
            return
        state = self.table_load_state[logical_name]
        if state['forms_loading'] or state['views_loading']:
            return
        if state['forms_loaded'] and state['views_loaded']:
            return
        
        state['forms_loading'] = True
        state['views_loading'] = True
        
        # Update tree to show loading
        values = list(self.selected_tree.item(logical_name, 'values'))
        values[1] = '(loading...)'
        values[2] = '(loading...)'
        self.selected_tree.item(logical_name, values=values)
        
        def do_load():
            try:
                forms = self.client.get_entity_forms(logical_name, include_xml=True)
                views = self.client.get_entity_views(logical_name)
                self.root.after(0, lambda: self._on_forms_views_loaded(logical_name, forms, views))
            except Exception as e:
                self.root.after(0, lambda: self._on_forms_views_error(logical_name, str(e)))
        
        threading.Thread(target=do_load, daemon=True).start()
    
    def _on_forms_views_loaded(self, logical_name: str, forms: List[Dict], views: List[Dict]):
        """Handle forms and views loaded"""
        if logical_name not in self.selected_tables:
            return
        
        self.table_forms[logical_name] = forms
        self.table_views[logical_name] = views
        state = self.table_load_state[logical_name]
        state['forms_loaded'] = True
        state['views_loaded'] = True
        state['forms_loading'] = False
        state['views_loading'] = False
        
        # Pre-select form from saved settings (or first available)
        selected_form_name = '(no forms)'
        if forms:
            saved_form_id = self.settings.table_forms.get(logical_name)
            selected_form = next((f for f in forms if f.get('formid') == saved_form_id), None) or forms[0]
            selected_form_name = selected_form.get('name', 'Unnamed')
        
        # Select saved view, default view, or first view
        default_view_name = '(no views)'
        if views:
            saved_view_id = self.settings.table_views.get(logical_name)
            selected_view = next((v for v in views if v.get('savedqueryid') == saved_view_id), None)
            if not selected_view:
                selected_view = next((v for v in views if v.get('isdefault')), views[0])
            default_view_name = selected_view.get('name', 'Unnamed')
            self.selected_views[logical_name] = selected_view.get('savedqueryid')
        
        # Update tree
        self.selected_tree.item(logical_name, values=(
            self.selected_tables[logical_name].get('DisplayName', ''),
            selected_form_name if forms else '(no forms)',
            default_view_name,
            len(self.selected_attributes.get(logical_name, set()))
        ))
        
        # If this table is selected, refresh UI
        selection = self.selected_tree.selection()
        if selection and selection[0] == logical_name:
            self._on_selected_table_click()
        self._autosave()
    
    def _on_forms_views_error(self, logical_name: str, error: str):
        """Handle forms/views load error"""
        state = self.table_load_state.get(logical_name)
        if state:
            state['forms_loading'] = False
            state['views_loading'] = False
        
        values = list(self.selected_tree.item(logical_name, 'values'))
        values[1] = '(error)'
        values[2] = '(error)'
        self.selected_tree.item(logical_name, values=values)
        print(f"Error loading forms/views for {logical_name}: {error}")
    
    def _on_remove_tables(self):
        """Remove selected tables from selection"""
        selection = self.selected_tree.selection()
        if not selection:
            return
        
        for item_id in selection:
            if item_id in self.selected_tables:
                del self.selected_tables[item_id]
            if item_id in self.table_forms:
                del self.table_forms[item_id]
            if item_id in self.table_views:
                del self.table_views[item_id]
            if item_id in self.table_attributes:
                del self.table_attributes[item_id]
            if item_id in self.selected_attributes:
                del self.selected_attributes[item_id]
            if item_id in self.selected_views:
                del self.selected_views[item_id]
            if item_id in self.table_load_state:
                del self.table_load_state[item_id]
            self.selected_tree.delete(item_id)
        
        # Clear attribute display
        for item in self.attr_tree.get_children():
            self.attr_tree.delete(item)
        self._update_table_count()
        self._autosave()
    
    def _on_selected_table_click(self, event=None):
        """Handle click on selected table"""
        selection = self.selected_tree.selection()
        if not selection:
            return
        
        logical_name = selection[0]
        if logical_name not in self.selected_tables:
            return

        # Check if forms/views need loading
        state = self.table_load_state.get(logical_name, {})
        if not state.get('forms_loaded') and not state.get('forms_loading'):
            self._load_forms_views_bulk([logical_name])
        
        # Update attributes display
        self._update_attributes_display(logical_name)
    
    def _on_table_tree_click(self, event):
        """Handle click on table tree - check if Edit button clicked"""
        column = self.selected_tree.identify_column(event.x)
        item_id = self.selected_tree.identify_row(event.y)
        
        if not item_id or item_id not in self.selected_tables:
            return
        
        # Check if clicking on edit button (column #5)
        if column == '#5':
            # Show both form and filter editors
            self._show_form_editor(item_id, event.x, event.y)
    
    def _show_form_editor(self, logical_name: str, x: int, y: int):
        """Show inline combo box editor for form selection"""
        forms = self.table_forms.get(logical_name, [])
        if not forms:
            messagebox.showinfo("No Forms", f"No forms available for this table.")
            return
        
        # Get cell bounding box for form column
        bbox = self.selected_tree.bbox(logical_name, '#2')
        if not bbox:
            return
        
        # Close any existing editor
        if self.editing_combo:
            self.editing_combo.destroy()
        
        # Create combo box at form cell position
        form_names = [f.get('name', 'Unnamed') for f in forms]
        current_form = self.selected_tree.item(logical_name, 'values')[1]
        
        combo = ttk.Combobox(self.selected_tree, values=form_names, state='readonly')
        combo.set(current_form)
        combo.place(x=bbox[0], y=bbox[1], width=bbox[2], height=bbox[3])
        combo.focus_set()
        combo.bind('<<ComboboxSelected>>', lambda e: self._finish_form_edit(logical_name, combo))
        combo.bind('<FocusOut>', lambda e: self._cancel_edit(combo))
        combo.bind('<Escape>', lambda e: self._cancel_edit(combo))
        
        self.editing_combo = combo
        
        # Open dropdown
        combo.event_generate('<Button-1>')
    
    def _cancel_edit(self, combo):
        """Cancel edit without saving"""
        if combo and combo.winfo_exists():
            combo.destroy()
        if self.editing_combo == combo:
            self.editing_combo = None
        
        self.editing_combo = combo
        
        # Open dropdown
        combo.event_generate('<Button-1>')
    
    def _finish_form_edit(self, logical_name: str, combo: ttk.Combobox):
        """Finish editing form selection"""
        form_name = combo.get()
        
        # Update tree display
        values = list(self.selected_tree.item(logical_name, 'values'))
        values[1] = form_name
        self.selected_tree.item(logical_name, values=values)
        
        # Clean up combo
        combo.destroy()
        self.editing_combo = None
        
        # Refresh attribute display to update "On Form" column
        if self.selected_tree.selection() and self.selected_tree.selection()[0] == logical_name:
            self._update_attributes_display(logical_name)
        
        self._autosave()
        
        # Now show filter/view editor
        self.root.after(100, lambda: self._show_filter_editor(logical_name))
    
    def _show_filter_editor(self, logical_name: str):
        """Show inline combo box editor for filter/view selection"""
        views = self.table_views.get(logical_name, [])
        if not views:
            # No views available, just finish
            return
        
        # Get cell bounding box for filter column
        bbox = self.selected_tree.bbox(logical_name, '#3')
        if not bbox:
            return
        
        # Close any existing editor
        if self.editing_combo:
            self.editing_combo.destroy()
        
        # Create combo box at cell position
        view_names = [v.get('name', 'Unnamed') for v in views]
        current_view = self.selected_tree.item(logical_name, 'values')[2]
        
        combo = ttk.Combobox(self.selected_tree, values=view_names, state='readonly')
        combo.set(current_view)
        combo.place(x=bbox[0], y=bbox[1], width=bbox[2], height=bbox[3])
        combo.focus_set()
        combo.bind('<<ComboboxSelected>>', lambda e: self._finish_filter_edit(logical_name, combo))
        combo.bind('<FocusOut>', lambda e: self._cancel_edit(combo))
        combo.bind('<Escape>', lambda e: self._cancel_edit(combo))
        
        self.editing_combo = combo
        
        # Open dropdown
        combo.event_generate('<Button-1>')
    
    def _finish_filter_edit(self, logical_name: str, combo: ttk.Combobox):
        """Finish editing filter/view selection"""
        view_name = combo.get()
        
        # Update selected view ID
        views = self.table_views.get(logical_name, [])
        for view in views:
            if view.get('name') == view_name:
                self.selected_views[logical_name] = view.get('savedqueryid')
                break
        
        # Update tree display
        values = list(self.selected_tree.item(logical_name, 'values'))
        values[2] = view_name
        self.selected_tree.item(logical_name, values=values)
        
        # Clean up combo
        combo.destroy()
        self.editing_combo = None
        
        self._autosave()
    
    def _update_attributes_display(self, logical_name: str):
        """Update the attributes tree for a table"""
        # Delete all items first to ensure fresh rendering
        for item in self.attr_tree.get_children():
            self.attr_tree.delete(item)
        
        attributes = self.table_attributes.get(logical_name, [])
        selected = self.selected_attributes.get(logical_name, set())
        required = self._get_required_attributes(logical_name)
        filter_text = self.attr_filter_var.get().lower()
        show_mode = self.attr_show_mode.get()
        
        print(f"DEBUG: Displaying {logical_name}, {len(selected)} selected attrs")  # DEBUG
        
        # Get fields on the selected form
        form_fields = set()
        selected_forms = self._get_selected_forms()
        if logical_name in selected_forms:
            form_id = selected_forms[logical_name]
            for form in self.table_forms.get(logical_name, []):
                if form.get('formid') == form_id and form.get('formxml'):
                    form_fields = DataverseClient.extract_fields_from_form_xml(form.get('formxml'))
                    break
        
        for attr in attributes:
            display = attr.get('DisplayName', '') or ''
            logical = attr.get('LogicalName', '')
            is_selected = logical in selected
            is_required = logical in required
            is_on_form = logical.lower() in form_fields
            
            # Apply show mode filter
            if show_mode == 'selected' and not is_selected:
                continue
            
            # Apply text filter
            if filter_text:
                if filter_text not in display.lower() and filter_text not in logical.lower():
                    continue
            
            # Show checkbox symbols: locked (required), checked, or unchecked
            if is_required:
                check_icon = '🔒'
            elif is_selected:
                check_icon = '☑'
            else:
                check_icon = '☐'
            
            # Show form indicator (read-only)
            form_icon = '✓' if is_on_form else ''
            
            tag = 'required' if is_required else ('selected' if is_selected else 'unselected')
            
            self.attr_tree.insert('', tk.END, iid=f"{logical_name}_{logical}", values=(
                check_icon,
                form_icon,
                display,
                logical,
                attr.get('AttributeType', '')
            ), tags=(tag,))
        
        # Update count in selected table tree
        if logical_name in self.selected_tables:
            values = list(self.selected_tree.item(logical_name, 'values'))
            values[3] = len(selected)
            self.selected_tree.item(logical_name, values=values)
    
    def _on_attr_filter_changed(self, *args):
        """Handle attribute filter change"""
        selection = self.selected_tree.selection()
        if selection:
            self._update_attributes_display(selection[0])
    
    def _on_attr_click(self, event):
        """Handle single click on attribute - toggle checkbox for any column"""
        item_id = self.attr_tree.identify_row(event.y)
        if not item_id:
            return
        
        # Get current table
        table_selection = self.selected_tree.selection()
        if not table_selection:
            return
        
        logical_name = table_selection[0]
        
        # Extract attribute name from item_id (format: tablename_attrname)
        if '_' in item_id:
            attr_name = item_id.split('_', 1)[1]
        else:
            return
        
        # Check if this is a required attribute
        required = self._get_required_attributes(logical_name)
        if attr_name in required:
            # Don't allow deselecting required attributes
            return 'break'
        
        if logical_name not in self.selected_attributes:
            self.selected_attributes[logical_name] = set()
        
        # Toggle the attribute selection
        if attr_name in self.selected_attributes[logical_name]:
            self.selected_attributes[logical_name].discard(attr_name)
            print(f"DEBUG: Removed {attr_name}, now have {len(self.selected_attributes[logical_name])} attrs")  # DEBUG
        else:
            self.selected_attributes[logical_name].add(attr_name)
            print(f"DEBUG: Added {attr_name}, now have {len(self.selected_attributes[logical_name])} attrs")  # DEBUG
        
        self._update_attributes_display(logical_name)
        self._autosave()
        
        # Prevent default selection behavior
        return 'break'
    
    def _sort_treeview(self, tree: ttk.Treeview, col: str):
        """Sort treeview by column"""
        tree_id = id(tree)
        current_col, reverse = self.sort_state.get(tree_id, (None, False))
        
        # Toggle reverse if same column, otherwise start ascending
        if current_col == col:
            reverse = not reverse
        else:
            reverse = False
        
        self.sort_state[tree_id] = (col, reverse)
        
        # Get all items with their values
        items = [(tree.item(iid, 'values'), iid) for iid in tree.get_children('')]
        
        # Find column index
        columns = tree['columns']
        col_idx = columns.index(col) if col in columns else 0
        
        # Sort
        def sort_key(item):
            val = item[0][col_idx] if len(item[0]) > col_idx else ''
            # Try numeric sort for numbers
            try:
                return (0, int(val))
            except (ValueError, TypeError):
                return (1, str(val).lower())
        
        items.sort(key=sort_key, reverse=reverse)
        
        # Reorder items
        for idx, (values, iid) in enumerate(items):
            tree.move(iid, '', idx)
    
    def _on_attr_toggle(self, event=None):
        """Toggle attribute selection on double-click of checkbox column"""
        # For double-click, only toggle if clicking on checkbox column
        if event:
            column = self.attr_tree.identify_column(event.x)
            if column != '#1':  # Only toggle on checkbox column
                return
        
        selection = self.attr_tree.selection()
        if not selection:
            return
        
        # Get current table
        table_selection = self.selected_tree.selection()
        if not table_selection:
            return
        
        logical_name = table_selection[0]
        required = self._get_required_attributes(logical_name)
        
        for item_id in selection:
            parts = item_id.split('_', 1)
            if len(parts) > 1:
                attr_name = parts[1]
                
                # Don't allow toggling required attributes
                if attr_name in required:
                    continue
                
                if logical_name not in self.selected_attributes:
                    self.selected_attributes[logical_name] = set()
                
                if attr_name in self.selected_attributes[logical_name]:
                    self.selected_attributes[logical_name].discard(attr_name)
                else:
                    self.selected_attributes[logical_name].add(attr_name)
        
        self._update_attributes_display(logical_name)
        self._autosave()
    
    def _on_select_all_attrs(self):
        """Select all visible attributes"""
        selection = self.selected_tree.selection()
        if not selection:
            return
        
        logical_name = selection[0]
        if logical_name not in self.selected_attributes:
            self.selected_attributes[logical_name] = set()
        
        for item_id in self.attr_tree.get_children():
            parts = item_id.split('_', 1)
            if len(parts) > 1:
                self.selected_attributes[logical_name].add(parts[1])
        
        self._update_attributes_display(logical_name)
        self._autosave()
    
    def _on_deselect_all_attrs(self):
        """Deselect all attributes except required ones"""
        selection = self.selected_tree.selection()
        if not selection:
            return
        
        logical_name = selection[0]
        # Keep only required attributes
        required = self._get_required_attributes(logical_name)
        self.selected_attributes[logical_name] = required.copy()
        self._update_attributes_display(logical_name)
        self._autosave()
    
    def _on_select_form_attrs(self):
        """Select attributes from the selected form"""
        selection = self.selected_tree.selection()
        if not selection:
            return
        
        logical_name = selection[0]
        
        # Get the form name from the tree
        values = self.selected_tree.item(logical_name, 'values')
        if len(values) < 2:
            return
        
        form_name = values[1]  # Form column
        
        if not form_name or form_name in ['(not loaded)', '(loading...)', '(no forms)']:
            messagebox.showinfo("No Form", "Please select a form first by clicking the Edit button.")
            return
        
        # Find the form
        forms = self.table_forms.get(logical_name, [])
        form = next((f for f in forms if f.get('name') == form_name), None)
        
        if not form:
            return
        
        # Check if we have form XML
        if form.get('formxml'):
            # Use cached form XML
            self._apply_form_fields(logical_name, form.get('formxml'))
        else:
            # Need to fetch form XML
            self._set_status("Loading form fields...")
            
            def do_load():
                try:
                    # Fetch form XML
                    form_xml = self.client.get_form_xml(form.get('formid'))
                    # Cache it
                    form['formxml'] = form_xml
                    self.root.after(0, lambda: self._apply_form_fields(logical_name, form_xml))
                except Exception as e:
                    self.root.after(0, lambda: self._set_status(f"Error loading form: {e}"))
            
            threading.Thread(target=do_load, daemon=True).start()
    
    def _apply_form_fields(self, logical_name: str, form_xml: str):
        """Apply form fields to attribute selection"""
        # Get fields from form
        form_fields = DataverseClient.extract_fields_from_form_xml(form_xml)
        if logical_name not in self.selected_attributes:
            self.selected_attributes[logical_name] = set()
        
        attributes = self.table_attributes.get(logical_name, [])
        
        for attr in attributes:
            if attr.get('LogicalName', '').lower() in form_fields:
                self.selected_attributes[logical_name].add(attr.get('LogicalName'))
        
        # Ensure required attributes are always included
        required = self._get_required_attributes(logical_name)
        self.selected_attributes[logical_name].update(required)
        
        self._update_attributes_display(logical_name)
        self._set_status(f"Selected {len(form_fields)} fields from form")
        self._autosave()
    
    def _on_browse_output(self):
        """Browse for output folder"""
        folder = filedialog.askdirectory(
            title="Select Output Folder",
            initialdir=self.output_var.get() or os.path.dirname(__file__)
        )
        if folder:
            self.output_var.set(folder)
            self._autosave()
    
    def _on_export(self):
        """Export metadata to JSON"""
        if not self.selected_tables:
            messagebox.showerror("Error", "No tables selected")
            return
        
        project_name = self.project_var.get().strip()
        if not project_name:
            messagebox.showerror("Error", "Please enter a project name")
            return
        
        output_folder = self.output_var.get().strip()
        if not output_folder:
            # Default to Reports/ProjectName/Metadata
            base_dir = os.path.dirname(os.path.dirname(__file__))
            output_folder = os.path.join(base_dir, "Reports", project_name, "Metadata")
            self.output_var.set(output_folder)
        
        self._set_status("Preparing export...")
        self.export_btn.config(state='disabled')
        
        def do_export():
            try:
                # Build metadata structure
                metadata = {
                    'Environment': self.url_var.get(),
                    'Solution': self.solution_var.get().split(' - ')[0] if self.solution_var.get() else '',
                    'ProjectName': project_name,
                    'Tables': []
                }
                
                total_tables = len(self.selected_tables)
                
                for idx, (logical_name, table_data) in enumerate(self.selected_tables.items()):
                    self.root.after(0, lambda n=logical_name, i=idx: self._set_status(f"Exporting {n} ({i+1}/{total_tables})..."))
                    
                    # Get selected form
                    forms = self.table_forms.get(logical_name, [])
                    selected_form_name = ''
                    for item_id in self.selected_tree.get_children():
                        if item_id == logical_name:
                            selected_form_name = self.selected_tree.item(item_id, 'values')[1]
                            break
                    
                    form_details = []
                    for form in forms:
                        if form.get('name') == selected_form_name:
                            # Fetch form XML only now at export time
                            try:
                                form_xml = self.client.get_form_xml(form.get('formid'))
                                field_count = len(DataverseClient.extract_fields_from_form_xml(form_xml))
                            except:
                                field_count = 0
                            
                            form_details.append({
                                'FormId': form.get('formid'),
                                'FormName': form.get('name'),
                                'FieldCount': field_count
                            })
                            break
                    
                    # Get selected view with FetchXML
                    selected_view_name = ''
                    view_details = None
                    for item_id in self.selected_tree.get_children():
                        if item_id == logical_name:
                            selected_view_name = self.selected_tree.item(item_id, 'values')[2]
                            break
                    
                    views = self.table_views.get(logical_name, [])
                    for view in views:
                        if view.get('name') == selected_view_name:
                            # Fetch FetchXML only now at export time
                            try:
                                fetch_xml = self.client.get_view_fetchxml(view.get('savedqueryid'))
                            except:
                                fetch_xml = None
                            
                            view_details = {
                                'ViewId': view.get('savedqueryid'),
                                'ViewName': view.get('name'),
                                'FetchXml': fetch_xml
                            }
                            break
                    
                    # Get selected attributes
                    selected_attr_names = self.selected_attributes.get(logical_name, set())
                    all_attrs = self.table_attributes.get(logical_name, [])
                    
                    attributes = []
                    for attr in all_attrs:
                        if attr.get('LogicalName') in selected_attr_names:
                            attributes.append({
                                'LogicalName': attr.get('LogicalName'),
                                'SchemaName': attr.get('SchemaName'),
                                'DisplayName': attr.get('DisplayName'),
                                'AttributeType': attr.get('AttributeType'),
                                'IsCustom': attr.get('IsCustom', False)
                            })
                    
                    metadata['Tables'].append({
                        'LogicalName': table_data.get('LogicalName'),
                        'DisplayName': table_data.get('DisplayName'),
                        'SchemaName': table_data.get('SchemaName'),
                        'ObjectTypeCode': table_data.get('ObjectTypeCode'),
                        'PrimaryIdAttribute': table_data.get('PrimaryIdAttribute'),
                        'PrimaryNameAttribute': table_data.get('PrimaryNameAttribute'),
                        'Forms': form_details,
                        'View': view_details,
                        'Attributes': attributes
                    })
                
                # Save to file
                os.makedirs(output_folder, exist_ok=True)
                output_file = os.path.join(output_folder, f"{project_name} Metadata Dictionary.json")
                
                with open(output_file, 'w', encoding='utf-8') as f:
                    json.dump(metadata, f, indent=2, ensure_ascii=False)
                
                # Also save DataverseURL.txt
                url_file = os.path.join(output_folder, "DataverseURL.txt")
                with open(url_file, 'w', encoding='utf-8') as f:
                    f.write(self.url_var.get())
                
                self.root.after(0, lambda: self._on_export_complete(output_file, metadata))
                
            except Exception as e:
                self.root.after(0, lambda: self._on_export_error(str(e)))
        
        threading.Thread(target=do_export, daemon=True).start()
    
    def _on_export_complete(self, output_file: str, metadata: dict):
        """Handle export completion"""
        self.export_btn.config(state='normal')
        self._set_status(f"Exported to {output_file}")
        self._save_settings()
        
        total_attrs = sum(len(t.get('Attributes', [])) for t in metadata['Tables'])
        messagebox.showinfo(
            "Export Complete",
            f"Metadata exported successfully!\n\n"
            f"File: {output_file}\n"
            f"Tables: {len(metadata['Tables'])}\n"
            f"Total Attributes: {total_attrs}"
        )
    
    def _on_export_error(self, error: str):
        """Handle export error"""
        self.export_btn.config(state='normal')
        self._set_status("Export failed")
        messagebox.showerror("Export Error", f"Failed to export:\n{error}")
    
    def _on_close(self):
        """Handle window close"""
        self._save_settings()
        self.root.destroy()
    
    def run(self):
        """Run the application"""
        self.root.mainloop()


def main():
    """Main entry point"""
    app = DataverseMetadataApp()
    app.run()


if __name__ == "__main__":
    main()
