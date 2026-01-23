"""
Extract metadata directly from Dataverse using Web API
This script authenticates to a Dataverse environment and extracts table and field metadata
from a specified solution, including fields from main forms.
"""

import requests
import json
import sys
import os
from typing import Dict, List, Set, Optional
from urllib.parse import quote

class DataverseMetadataExtractor:
    """Extract metadata from Dataverse using the Web API"""
    
    def __init__(self, environment_url: str, access_token: str):
        """
        Initialize the extractor
        
        Args:
            environment_url: Dataverse environment URL (e.g., https://yourorg.crm.dynamics.com)
            access_token: OAuth access token for authentication
        """
        self.base_url = environment_url.rstrip('/')
        self.api_url = f"{self.base_url}/api/data/v9.2"
        self.headers = {
            "Authorization": f"Bearer {access_token}",
            "OData-MaxVersion": "4.0",
            "OData-Version": "4.0",
            "Accept": "application/json",
            "Content-Type": "application/json; charset=utf-8",
            "Prefer": "odata.include-annotations=*"
        }
    
    def get_solution_tables(self, solution_name: str) -> List[Dict]:
        """
        Get all tables (entities) in a solution
        
        Args:
            solution_name: Unique name of the solution
            
        Returns:
            List of table metadata dictionaries
        """
        print(f"Fetching tables from solution: {solution_name}...")
        
        # Query to get all entity metadata IDs in the solution
        query = (
            f"{self.api_url}/solutions?"
            f"$filter=uniquename eq '{solution_name}'"
            f"&$select=solutionid,friendlyname"
        )
        
        response = requests.get(query, headers=self.headers)
        response.raise_for_status()
        solutions = response.json().get('value', [])
        
        if not solutions:
            raise ValueError(f"Solution '{solution_name}' not found")
        
        solution_id = solutions[0]['solutionid']
        print(f"Found solution: {solutions[0].get('friendlyname', solution_name)} ({solution_id})")
        
        # Get solution components that are entities (ComponentType = 1)
        query = (
            f"{self.api_url}/solutioncomponents?"
            f"$filter=_solutionid_value eq {solution_id} and componenttype eq 1"
            f"&$select=objectid"
        )
        
        response = requests.get(query, headers=self.headers)
        response.raise_for_status()
        components = response.json().get('value', [])
        
        entity_ids = [comp['objectid'] for comp in components]
        print(f"Found {len(entity_ids)} tables in solution")
        
        # Get entity metadata for each table
        tables = []
        for entity_id in entity_ids:
            try:
                entity_metadata = self.get_entity_metadata(entity_id)
                if entity_metadata:
                    tables.append(entity_metadata)
            except Exception as e:
                print(f"Warning: Could not retrieve metadata for entity {entity_id}: {e}")
        
        return tables
    
    def get_entity_metadata(self, entity_id: str) -> Optional[Dict]:
        """
        Get metadata for a specific entity
        
        Args:
            entity_id: MetadataId of the entity
            
        Returns:
            Dictionary with entity metadata
        """
        query = f"{self.api_url}/EntityDefinitions({entity_id})"
        
        response = requests.get(query, headers=self.headers)
        response.raise_for_status()
        entity = response.json()
        
        # Skip virtual, intersection tables, and activity tables typically
        if entity.get('IsActivity') or entity.get('IsIntersect'):
            return None
        
        logical_name = entity.get('LogicalName')
        display_name = entity.get('DisplayName', {}).get('UserLocalizedLabel', {}).get('Label', logical_name)
        
        print(f"  Processing: {display_name} ({logical_name})")
        
        return {
            'LogicalName': logical_name,
            'DisplayName': display_name,
            'SchemaName': entity.get('SchemaName'),
            'ObjectTypeCode': entity.get('ObjectTypeCode'),
            'PrimaryIdAttribute': entity.get('PrimaryIdAttribute'),
            'PrimaryNameAttribute': entity.get('PrimaryNameAttribute'),
            'MetadataId': entity_id
        }
    
    def get_main_forms_for_entity(self, entity_logical_name: str) -> List[Dict]:
        """
        Get main forms for an entity
        
        Args:
            entity_logical_name: Logical name of the entity
            
        Returns:
            List of form metadata
        """
        # FormType 2 = Main forms
        query = (
            f"{self.api_url}/systemforms?"
            f"$filter=objecttypecode eq '{entity_logical_name}' and type eq 2 and iscustomizable/Value eq true"
            f"&$select=formid,name,formxml"
            f"&$orderby=name"
        )
        
        response = requests.get(query, headers=self.headers)
        response.raise_for_status()
        return response.json().get('value', [])
    
    def extract_fields_from_form_xml(self, form_xml: str) -> Set[str]:
        """
        Extract field logical names from form XML
        
        Args:
            form_xml: XML string of the form definition
            
        Returns:
            Set of field logical names
        """
        import xml.etree.ElementTree as ET
        
        fields = set()
        
        try:
            root = ET.fromstring(form_xml)
            
            # Find all control elements with datafieldname attribute
            for control in root.findall(".//control[@datafieldname]"):
                field_name = control.get('datafieldname')
                if field_name:
                    fields.add(field_name.lower())
            
            # Also check for 'id' attribute which sometimes contains field names
            for control in root.findall(".//control[@id]"):
                control_id = control.get('id')
                # Sometimes the id is the field name
                if control_id and not control_id.startswith('header_') and not control_id.startswith('footer_'):
                    fields.add(control_id.lower())
                    
        except Exception as e:
            print(f"    Warning: Error parsing form XML: {e}")
        
        return fields
    
    def get_entity_attributes(self, entity_logical_name: str, form_fields: Set[str]) -> List[Dict]:
        """
        Get attribute metadata for fields in forms
        
        Args:
            entity_logical_name: Logical name of the entity
            form_fields: Set of field logical names from forms
            
        Returns:
            List of attribute metadata
        """
        query = (
            f"{self.api_url}/EntityDefinitions(LogicalName='{entity_logical_name}')/Attributes?"
            f"$select=LogicalName,SchemaName,DisplayName,AttributeType,IsValidForRead,IsCustomAttribute"
        )
        
        response = requests.get(query, headers=self.headers)
        response.raise_for_status()
        all_attributes = response.json().get('value', [])
        
        # Filter to only attributes that are in forms or are standard important fields
        important_fields = {'createdon', 'modifiedon', 'createdby', 'modifiedby', 'ownerid', 'statecode', 'statuscode'}
        
        filtered_attributes = []
        for attr in all_attributes:
            logical_name = attr.get('LogicalName', '').lower()
            
            # Include if it's in a form or is an important standard field
            if logical_name in form_fields or logical_name in important_fields:
                if attr.get('IsValidForRead'):
                    display_name = attr.get('DisplayName', {}).get('UserLocalizedLabel', {})
                    display_label = display_name.get('Label') if display_name else attr.get('SchemaName')
                    
                    filtered_attributes.append({
                        'LogicalName': attr.get('LogicalName'),
                        'SchemaName': attr.get('SchemaName'),
                        'DisplayName': display_label,
                        'AttributeType': attr.get('AttributeType'),
                        'IsCustom': attr.get('IsCustomAttribute', False)
                    })
        
        return filtered_attributes
    
    def extract_metadata(self, solution_name: str, output_folder: str) -> Dict:
        """
        Extract complete metadata from a solution
        
        Args:
            solution_name: Unique name of the solution
            output_folder: Folder to save the JSON output
            
        Returns:
            Dictionary with complete metadata
        """
        print("="*80)
        print("DATAVERSE METADATA EXTRACTION")
        print("="*80)
        print(f"Environment: {self.base_url}")
        print(f"Solution: {solution_name}")
        print("="*80)
        
        # Get all tables in the solution
        tables = self.get_solution_tables(solution_name)
        
        print(f"\n{'='*80}")
        print("EXTRACTING FIELDS FROM MAIN FORMS")
        print(f"{'='*80}")
        
        # For each table, get forms and fields
        metadata = {
            'Environment': self.base_url,
            'Solution': solution_name,
            'Tables': []
        }
        
        for table in tables:
            logical_name = table['LogicalName']
            display_name = table['DisplayName']
            
            print(f"\n{display_name} ({logical_name}):")
            
            # Get main forms
            forms = self.get_main_forms_for_entity(logical_name)
            print(f"  Found {len(forms)} main form(s)")
            
            # Extract fields from all forms
            all_form_fields = set()
            form_details = []
            
            for form in forms:
                form_name = form.get('name')
                form_xml = form.get('formxml', '')
                
                fields_in_form = self.extract_fields_from_form_xml(form_xml)
                all_form_fields.update(fields_in_form)
                
                form_details.append({
                    'FormId': form.get('formid'),
                    'FormName': form_name,
                    'FieldCount': len(fields_in_form)
                })
                
                print(f"    - {form_name}: {len(fields_in_form)} fields")
            
            # Get attribute metadata for fields in forms
            if all_form_fields:
                attributes = self.get_entity_attributes(logical_name, all_form_fields)
                print(f"  Total unique fields: {len(attributes)}")
                
                table['Forms'] = form_details
                table['Attributes'] = attributes
                metadata['Tables'].append(table)
            else:
                print(f"  Warning: No fields found in forms")
        
        # Save to JSON file
        os.makedirs(output_folder, exist_ok=True)
        output_file = os.path.join(output_folder, f"{solution_name} Metadata Dictionary.json")
        
        with open(output_file, 'w', encoding='utf-8') as f:
            json.dump(metadata, f, indent=2, ensure_ascii=False)
        
        print(f"\n{'='*80}")
        print(f"METADATA SAVED TO: {output_file}")
        print(f"{'='*80}")
        print(f"Total Tables: {len(metadata['Tables'])}")
        total_fields = sum(len(t.get('Attributes', [])) for t in metadata['Tables'])
        print(f"Total Fields: {total_fields}")
        print(f"{'='*80}")
        
        return metadata


def get_access_token_interactive(environment_url: str) -> str:
    """
    Get an access token using interactive authentication (MSAL)
    
    Args:
        environment_url: Dataverse environment URL
        
    Returns:
        Access token string
    """
    try:
        from msal import PublicClientApplication
    except ImportError:
        print("ERROR: The 'msal' library is required for authentication.")
        print("Install it with: pip install msal")
        sys.exit(1)
    
    # Microsoft Dataverse client ID (public client for authentication)
    client_id = "51f81489-12ee-4a9e-aaae-a2591f45987d"  # Common Dataverse client
    authority = "https://login.microsoftonline.com/organizations"
    
    # Extract resource URL from environment
    import re
    match = re.search(r'https://([^/]+)', environment_url)
    if not match:
        raise ValueError(f"Invalid environment URL: {environment_url}")
    
    resource_url = f"https://{match.group(1)}"
    scopes = [f"{resource_url}/.default"]
    
    app = PublicClientApplication(
        client_id=client_id,
        authority=authority
    )
    
    print("Authenticating to Dataverse...")
    print("A browser window will open for you to sign in.")
    
    # Try to get token from cache first
    accounts = app.get_accounts()
    if accounts:
        result = app.acquire_token_silent(scopes, account=accounts[0])
        if result and 'access_token' in result:
            print("Using cached authentication")
            return result['access_token']
    
    # Interactive login
    result = app.acquire_token_interactive(scopes=scopes)
    
    if 'access_token' in result:
        print("Authentication successful!")
        return result['access_token']
    else:
        error = result.get('error', 'Unknown error')
        error_desc = result.get('error_description', 'No description')
        raise Exception(f"Authentication failed: {error} - {error_desc}")


def main():
    """Main entry point"""
    if len(sys.argv) < 3:
        print("Usage: python extract_metadata_from_dataverse.py <environment_url> <solution_name> [output_folder]")
        print("\nExamples:")
        print("  python extract_metadata_from_dataverse.py https://yourorg.crm.dynamics.com CoreAI")
        print("  python extract_metadata_from_dataverse.py https://yourorg.crm.dynamics.com CoreAI 'Reports/MyProject/Metadata'")
        print("\nParameters:")
        print("  environment_url: Your Dataverse environment URL")
        print("  solution_name: Unique name of the solution (not the display name)")
        print("  output_folder: (Optional) Folder to save JSON output. Default: current directory")
        sys.exit(1)
    
    environment_url = sys.argv[1].rstrip('/')
    solution_name = sys.argv[2]
    output_folder = sys.argv[3] if len(sys.argv) > 3 else '.'
    
    try:
        # Authenticate
        access_token = get_access_token_interactive(environment_url)
        
        # Extract metadata
        extractor = DataverseMetadataExtractor(environment_url, access_token)
        metadata = extractor.extract_metadata(solution_name, output_folder)
        
        print("\n✓ Extraction complete!")
        
    except Exception as e:
        print(f"\n✗ ERROR: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)


if __name__ == "__main__":
    main()
