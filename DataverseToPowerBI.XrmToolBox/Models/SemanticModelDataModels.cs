// ===================================================================================
// SemanticModelDataModels.cs - XrmToolBox-Specific Data Models
// ===================================================================================
//
// PURPOSE:
// Defines data models specific to the XrmToolBox plugin that are not shared with
// the Core library. These models support star-schema relationship configuration
// and serialization for settings persistence.
//
// SHARED MODELS:
// DateTableConfig and DateTimeFieldConfig are imported from Core.Models to avoid
// duplication and type conflicts. All other table, attribute, and solution models
// come from the Core.Models namespace.
//
// MODELS DEFINED HERE:
//
// ExportRelationship:
//   Represents a many-to-one relationship between tables in the star schema.
//   - SourceTable: The "many" side (fact or dimension with lookup)
//   - SourceAttribute: The lookup field name
//   - TargetTable: The "one" side (dimension table)
//   - IsActive: Whether this is the active relationship (one per target)
//   - IsSnowflake: True for dimension-to-parent-dimension relationships
//   - AssumeReferentialIntegrity: True if lookup is required (performance hint)
//
// SERIALIZATION:
// All models use DataContract/DataMember attributes for JSON serialization
// via DataContractJsonSerializer, which is compatible with .NET Framework 4.6.2.
//
// ===================================================================================

using System;
using System.Collections.Generic;

namespace DataverseToPowerBI.XrmToolBox.Models
{
    // NOTE: DateTableConfig and DateTimeFieldConfig are imported from Core.Models
    // to avoid duplication and type conflicts

    /// <summary>
    /// Represents a relationship for TMDL export in the FactDimensionSelectorForm.
    /// Uses DataContract serialization for XrmToolBox settings persistence.
    /// </summary>
    /// <remarks>
    /// This model intentionally duplicates some properties from
    /// <see cref="DataverseToPowerBI.Core.Models.RelationshipConfig"/> in Core.
    /// The models use different serialization strategies and cannot be consolidated
    /// without breaking existing saved configurations.
    /// </remarks>
    [System.Runtime.Serialization.DataContract]
    public class ExportRelationship
    {
        /// <summary>Source (Many-side) table logical name — the fact or dimension table containing the lookup.</summary>
        [System.Runtime.Serialization.DataMember]
        public string SourceTable { get; set; } = "";
        /// <summary>Lookup attribute logical name on the source table.</summary>
        [System.Runtime.Serialization.DataMember]
        public string SourceAttribute { get; set; } = "";
        /// <summary>Target (One-side) table logical name — the dimension table referenced by the lookup.</summary>
        [System.Runtime.Serialization.DataMember]
        public string TargetTable { get; set; } = "";
        /// <summary>User-friendly display name for the relationship.</summary>
        [System.Runtime.Serialization.DataMember]
        public string DisplayName { get; set; } = "";
        /// <summary>Whether the relationship is active in the Power BI model. Inactive relationships require DAX USERELATIONSHIP().</summary>
        [System.Runtime.Serialization.DataMember]
        public bool IsActive { get; set; } = true;
        /// <summary>True if this is a snowflake relationship (Dimension → Parent Dimension).</summary>
        [System.Runtime.Serialization.DataMember]
        public bool IsSnowflake { get; set; } = false;
        /// <summary>Snowflake depth: 0 = Direct, 1 = Snowflake, 2 = Double Snowflake.</summary>
        [System.Runtime.Serialization.DataMember]
        public int SnowflakeLevel { get; set; } = 0;
        /// <summary>True if the lookup field is required (not nullable), enabling referential integrity optimization in DirectQuery.</summary>
        [System.Runtime.Serialization.DataMember]
        public bool AssumeReferentialIntegrity { get; set; } = false;
    }
}
