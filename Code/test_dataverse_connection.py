"""
Quick test script to verify Dataverse connection and list available solutions
This can help you find the correct solution name before running the full extraction
"""

import sys
from extract_metadata_from_dataverse import get_access_token_interactive
import requests


def list_solutions(environment_url: str, access_token: str):
    """List all solutions in the environment"""
    
    api_url = f"{environment_url.rstrip('/')}/api/data/v9.2"
    headers = {
        "Authorization": f"Bearer {access_token}",
        "OData-MaxVersion": "4.0",
        "OData-Version": "4.0",
        "Accept": "application/json",
        "Content-Type": "application/json; charset=utf-8"
    }
    
    print("=" * 80)
    print("AVAILABLE SOLUTIONS")
    print("=" * 80)
    
    # Get all solutions (excluding system ones)
    query = (
        f"{api_url}/solutions?"
        f"$select=solutionid,uniquename,friendlyname,version,ismanaged,publisherid"
        f"&$filter=isvisible eq true"
        f"&$orderby=friendlyname"
    )
    
    response = requests.get(query, headers=headers)
    response.raise_for_status()
    solutions = response.json().get('value', [])
    
    print(f"\nFound {len(solutions)} solution(s):\n")
    print(f"{'Display Name':<40} {'Unique Name':<30} {'Version':<12} {'Managed'}")
    print("-" * 95)
    
    for solution in solutions:
        display_name = solution.get('friendlyname', 'N/A')
        unique_name = solution.get('uniquename', 'N/A')
        version = solution.get('version', 'N/A')
        is_managed = 'Yes' if solution.get('ismanaged') else 'No'
        
        # Truncate long names
        if len(display_name) > 38:
            display_name = display_name[:35] + "..."
        if len(unique_name) > 28:
            unique_name = unique_name[:25] + "..."
            
        print(f"{display_name:<40} {unique_name:<30} {version:<12} {is_managed}")
    
    print("\n" + "=" * 80)
    print("Use the 'Unique Name' value when running the metadata extraction script")
    print("=" * 80)


def test_connection(environment_url: str):
    """Test connection to Dataverse and list solutions"""
    
    try:
        print(f"Testing connection to: {environment_url}\n")
        
        # Authenticate
        access_token = get_access_token_interactive(environment_url)
        
        # List solutions
        list_solutions(environment_url, access_token)
        
        print("\n✓ Connection test successful!\n")
        print("Next steps:")
        print("  1. Choose a solution from the list above")
        print("  2. Run the extraction script:")
        print(f"     python extract_metadata_from_dataverse.py {environment_url} <solution_unique_name>")
        
    except Exception as e:
        print(f"\n✗ ERROR: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)


def main():
    """Main entry point"""
    
    if len(sys.argv) < 2:
        print("Usage: python test_dataverse_connection.py <environment_url>")
        print("\nExample:")
        print("  python test_dataverse_connection.py https://yourorg.crm.dynamics.com")
        print("\nThis script will:")
        print("  1. Test your connection to Dataverse")
        print("  2. List all available solutions")
        print("  3. Help you find the correct solution unique name for extraction")
        sys.exit(1)
    
    environment_url = sys.argv[1].rstrip('/')
    test_connection(environment_url)


if __name__ == "__main__":
    main()
