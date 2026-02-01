using System;
using System.Collections.Generic;

namespace DataverseToPowerBI.XrmToolBox.Models
{
    // NOTE: DateTableConfig and DateTimeFieldConfig are imported from Core.Models
    // to avoid duplication and type conflicts

    [System.Runtime.Serialization.DataContract]
    public class ExportRelationship
    {
        [System.Runtime.Serialization.DataMember]
        public string SourceTable { get; set; } = "";       // Fact or Dimension table (Many side)
        [System.Runtime.Serialization.DataMember]
        public string SourceAttribute { get; set; } = "";   // Lookup attribute
        [System.Runtime.Serialization.DataMember]
        public string TargetTable { get; set; } = "";       // Dimension table (One side)
        [System.Runtime.Serialization.DataMember]
        public string DisplayName { get; set; }
        [System.Runtime.Serialization.DataMember]
        public bool IsActive { get; set; } = true;
        [System.Runtime.Serialization.DataMember]
        public bool IsSnowflake { get; set; } = false;      // True if Dimension->ParentDimension
        [System.Runtime.Serialization.DataMember]
        public bool AssumeReferentialIntegrity { get; set; } = false;  // True if lookup field is required
    }
}
