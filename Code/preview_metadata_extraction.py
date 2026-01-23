"""
Preview what metadata will be extracted from a solution
Shows table and form counts without extracting full field details
"""

import sys
from extract_metadata_from_dataverse import (
    get_access_token_interactive,
    DataverseMetadataExtractor
)


def preview_extraction(environment_url: str, solution_name: str):
    """
    Preview metadata extraction - shows what will be extracted without doing full extraction
    
    Args:
        environment_url: Dataverse environment URL
        solution_name: Unique name of the solution
    """
    
    try:
        print("=" * 80)
        print("METADATA EXTRACTION PREVIEW")
        print("=" * 80)
        print(f"Environment: {environment_url}")
        print(f"Solution: {solution_name}")
        print("=" * 80)
        
        # Authenticate
        access_token = get_access_token_interactive(environment_url)
        
        # Create extractor
        extractor = DataverseMetadataExtractor(environment_url, access_token)
        
        # Get tables
        tables = extractor.get_solution_tables(solution_name)
        
        print(f"\n{'=' * 80}")
        print("TABLES AND FORMS SUMMARY")
        print(f"{'=' * 80}\n")
        
        print(f"{'Table Display Name':<40} {'Logical Name':<25} {'Forms'}")
        print("-" * 80)
        
        total_forms = 0
        tables_with_forms = 0
        tables_without_forms = 0
        
        for table in tables:
            logical_name = table['LogicalName']
            display_name = table['DisplayName']
            
            # Get form count
            forms = extractor.get_main_forms_for_entity(logical_name)
            form_count = len(forms)
            total_forms += form_count
            
            if form_count > 0:
                tables_with_forms += 1
            else:
                tables_without_forms += 1
            
            # Truncate long names
            if len(display_name) > 38:
                display_name = display_name[:35] + "..."
            if len(logical_name) > 23:
                logical_name = logical_name[:20] + "..."
            
            status = f"{form_count}" if form_count > 0 else "⚠️  None"
            print(f"{display_name:<40} {logical_name:<25} {status}")
        
        print("\n" + "=" * 80)
        print("SUMMARY")
        print("=" * 80)
        print(f"Total Tables in Solution: {len(tables)}")
        print(f"Tables with Forms: {tables_with_forms}")
        print(f"Tables without Forms: {tables_without_forms}")
        print(f"Total Main Forms: {total_forms}")
        print("=" * 80)
        
        if tables_without_forms > 0:
            print("\n⚠️  WARNING: Some tables have no main forms")
            print("   These tables will be skipped during extraction")
            print("   Consider adding forms or using a different solution scope")
        
        print("\n✓ Preview complete!")
        print("\nTo extract full metadata, run:")
        print(f"  python extract_metadata_from_dataverse.py {environment_url} {solution_name}")
        
    except Exception as e:
        print(f"\n✗ ERROR: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)


def main():
    """Main entry point"""
    
    if len(sys.argv) < 3:
        print("Usage: python preview_metadata_extraction.py <environment_url> <solution_name>")
        print("\nExample:")
        print("  python preview_metadata_extraction.py https://yourorg.crm.dynamics.com CoreAI")
        print("\nThis script will:")
        print("  1. Connect to your Dataverse environment")
        print("  2. Show all tables in the solution")
        print("  3. Display form counts for each table")
        print("  4. Provide a summary without extracting full field details")
        sys.exit(1)
    
    environment_url = sys.argv[1].rstrip('/')
    solution_name = sys.argv[2]
    
    preview_extraction(environment_url, solution_name)


if __name__ == "__main__":
    main()
