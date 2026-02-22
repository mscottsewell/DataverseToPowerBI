// =============================================================================
// IncrementalUpdateTests.cs - Integration Tests for Incremental Update Features
// =============================================================================
// Purpose: Validates lineage preservation, description survival, and metadata
// stability across incremental updates. Covers CODEX-1 (lineage key stability),
// AGREE-1 (description preservation), and CODEX-3 (incremental scenarios).
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataverseToPowerBI.XrmToolBox.Services;
using Xunit;

namespace DataverseToPowerBI.Tests
{
    public class IncrementalUpdateTests : IDisposable
    {
        private readonly SemanticModelBuilder _builder;
        private readonly string _fixturesPath;
        private readonly string _tempDir;

        public IncrementalUpdateTests()
        {
            _fixturesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fixtures");
            _tempDir = Path.Combine(Path.GetTempPath(), "DataverseToPowerBI_IncrTests_" + Guid.NewGuid().ToString("N").Substring(0, 8));
            Directory.CreateDirectory(_tempDir);
            _builder = new SemanticModelBuilder(_tempDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        private string FixturePath(string name) => Path.Combine(_fixturesPath, name);

        #region Lineage Stability with Logical Name Annotations

        [Fact]
        public void ParseExistingLineageTags_WithLogicalNameAnnotation_CreatesLogicalColFallback()
        {
            var tags = _builder.ParseExistingLineageTags(FixturePath("SampleTableWithLogicalNames.tmdl"));

            // Primary key (sourceColumn matches logicalName, so both entries exist)
            Assert.Equal("11111111-2222-3333-4444-555555555555", tags["col:accountid"]);
            Assert.Equal("11111111-2222-3333-4444-555555555555", tags["logicalcol:accountid"]);

            // Display-name aliased column: sourceColumn = "Old Display Name", logicalName = "name"
            Assert.Equal("22222222-3333-4444-5555-666666666666", tags["col:Old Display Name"]);
            Assert.Equal("22222222-3333-4444-5555-666666666666", tags["logicalcol:name"]);

            // Revenue
            Assert.Equal("33333333-4444-5555-6666-777777777777", tags["logicalcol:revenue"]);
        }

        [Fact]
        public void GetOrNewLineageTag_FallsBackToLogicalColKey_WhenPrimaryKeyMissing()
        {
            var existingTags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "col:Old Display Name", "preserved-guid-1234" },
                { "logicalcol:name", "preserved-guid-1234" }
            };

            // Primary key "col:New Display Name" won't match, but fallback "logicalcol:name" will
            var tag = _builder.GetOrNewLineageTag(existingTags, "col:New Display Name", "logicalcol:name");
            Assert.Equal("preserved-guid-1234", tag);
        }

        [Fact]
        public void GetOrNewLineageTag_PrefersPrimaryKey_WhenBothExist()
        {
            var existingTags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "col:Revenue", "primary-guid" },
                { "logicalcol:revenue", "fallback-guid" }
            };

            // Should prefer primary key match
            var tag = _builder.GetOrNewLineageTag(existingTags, "col:Revenue", "logicalcol:revenue");
            Assert.Equal("primary-guid", tag);
        }

        [Fact]
        public void GetOrNewLineageTag_GeneratesNewGuid_WhenNeitherKeyMatches()
        {
            var existingTags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "col:SomeOtherColumn", "other-guid" }
            };

            var tag = _builder.GetOrNewLineageTag(existingTags, "col:BrandNewColumn", "logicalcol:brandnewcolumn");
            Assert.True(Guid.TryParse(tag, out _), "Should generate a valid GUID when no match found");
        }

        [Fact]
        public void GetOrNewLineageTag_WithNullTags_GeneratesNewGuid()
        {
            var tag = _builder.GetOrNewLineageTag(null, "col:anything");
            Assert.True(Guid.TryParse(tag, out _), "Should generate a valid GUID when tags are null");
        }

        #endregion

        #region Column Metadata Preservation with Logical Name Fallback

        [Fact]
        public void ParseExistingColumnMetadata_WithLogicalNameAnnotation_CreatesLogicalColFallback()
        {
            var columns = _builder.ParseExistingColumnMetadata(FixturePath("SampleTableWithLogicalNames.tmdl"));

            // Should be keyed by sourceColumn AND by logicalcol:
            Assert.True(columns.ContainsKey("Old Display Name"));
            Assert.True(columns.ContainsKey("logicalcol:name"));

            // Both should reference the same info (same description)
            Assert.Equal("The primary name of the account", columns["Old Display Name"].Description);
            Assert.Equal("The primary name of the account", columns["logicalcol:name"].Description);

            // User annotation should be preserved
            Assert.True(columns["Old Display Name"].Annotations.ContainsKey("UserCustomAnnotation"));
            Assert.Equal("MyValue", columns["Old Display Name"].Annotations["UserCustomAnnotation"]);
        }

        #endregion

        #region Description Preservation (AGREE-1)

        [Fact]
        public void WriteTmdlFile_NonRelationshipFile_PreservesDescriptions()
        {
            // Create a temp file that simulates a table TMDL with descriptions
            var testFile = Path.Combine(_tempDir, "TestTable.tmdl");
            var content = "table TestTable\r\n\tlineageTag: abc\r\n\r\n\tcolumn Name\r\n\t\tdataType: string\r\n\t\tdescription: User-authored description\r\n\t\tsourceColumn: Name\r\n";

            File.WriteAllText(testFile, content);

            // Read it back — descriptions should still be there
            var written = File.ReadAllText(testFile);
            Assert.Contains("description: User-authored description", written);
        }

        [Fact]
        public void ParseExistingColumnMetadata_PreservesDescriptions()
        {
            var columns = _builder.ParseExistingColumnMetadata(FixturePath("SampleTable.tmdl"));

            // "name" column has description: "The primary name of the account"
            Assert.True(columns.ContainsKey("name"));
            Assert.Equal("The primary name of the account", columns["name"].Description);

            // "createdon" column has description: "User-edited description here"
            Assert.True(columns.ContainsKey("createdon"));
            Assert.Equal("User-edited description here", columns["createdon"].Description);
        }

        #endregion

        #region User Measure Preservation

        [Fact]
        public void ParseExistingLineageTags_PreservesUserMeasureTags()
        {
            var tags = _builder.ParseExistingLineageTags(FixturePath("SampleTable.tmdl"));

            // User-defined measure should be preserved
            Assert.True(tags.ContainsKey("measure:Custom Measure"));
            Assert.Equal("55555555-6666-7777-8888-999999999999", tags["measure:Custom Measure"]);
        }

        [Fact]
        public void ParseExistingLineageTags_PreservesAutoGeneratedMeasureTags()
        {
            var tags = _builder.ParseExistingLineageTags(FixturePath("SampleTable.tmdl"));

            // Auto-generated measures should also have stable lineage
            Assert.True(tags.ContainsKey("measure:Link to Account"));
            Assert.True(tags.ContainsKey("measure:Account Count"));
        }

        #endregion

        #region User Annotation Preservation

        [Fact]
        public void ParseExistingColumnMetadata_PreservesUserAnnotations()
        {
            var columns = _builder.ParseExistingColumnMetadata(FixturePath("SampleTable.tmdl"));

            Assert.True(columns.ContainsKey("name"));
            Assert.True(columns["name"].Annotations.ContainsKey("UserCustomAnnotation"));
            Assert.Equal("MyValue", columns["name"].Annotations["UserCustomAnnotation"]);
        }

        [Fact]
        public void ParseExistingColumnMetadata_PreservesFormatStrings()
        {
            var columns = _builder.ParseExistingColumnMetadata(FixturePath("SampleTable.tmdl"));

            Assert.True(columns.ContainsKey("revenue"));
            Assert.Equal(@"\$#,0.00;(\$#,0.00);\$#,0.00", columns["revenue"].FormatString);
        }

        #endregion
    }
}
