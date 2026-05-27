using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using DataverseToPowerBI.Core.Models;
using DataverseToPowerBI.XrmToolBox;
using Xunit;

namespace DataverseToPowerBI.Tests
{
    public class SemanticModelConfigSerializationTests
    {
        [Fact]
        public void SemanticModelConfig_Deserialization_MissingAliasProperty_DefaultsToTrue()
        {
            var json =
                "{" +
                "\"Name\":\"LegacyModel\"," +
                "\"DataverseUrl\":\"https://org.crm.dynamics.com\"," +
                "\"WorkingFolder\":\"C:\\\\Temp\\\\Model\"," +
                "\"TemplatePath\":\"C:\\\\Temp\\\\Template\"" +
                "}";

            var serializer = new DataContractJsonSerializer(typeof(SemanticModelConfig));
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var model = (SemanticModelConfig)serializer.ReadObject(ms)!;

            Assert.True(model.UseDisplayNameRenamesInPowerQuery);
        }

        [Fact]
        public void SemanticModelConfig_Deserialization_ExplicitFalse_IsPreserved()
        {
            var json =
                "{" +
                "\"Name\":\"CustomModel\"," +
                "\"DataverseUrl\":\"https://org.crm.dynamics.com\"," +
                "\"WorkingFolder\":\"C:\\\\Temp\\\\Model\"," +
                "\"TemplatePath\":\"C:\\\\Temp\\\\Template\"," +
                "\"UseDisplayNameAliasesInSql\":false" +
                "}";

            var serializer = new DataContractJsonSerializer(typeof(SemanticModelConfig));
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var model = (SemanticModelConfig)serializer.ReadObject(ms)!;

            Assert.False(model.UseDisplayNameRenamesInPowerQuery);
        }

        // -----------------------------------------------------------------------
        // CopyModel fidelity tests
        // Each test verifies a field that was previously dropped on copy.
        // -----------------------------------------------------------------------

        private SemanticModelManager CreateManager() => new SemanticModelManager(inMemory: true);

        [Fact]
        public void CopyModel_PreservesSelectedFieldViewIds()
        {
            var mgr = CreateManager();
            var src = MakeModel("Source");
            src.PluginSettings.SelectedFieldViewIds = new Dictionary<string, string>
            {
                ["account"] = "fv-guid-1",
                ["contact"] = "fv-guid-2"
            };
            mgr.CreateModel(src);

            mgr.CopyModel("Source", "Copy");

            var copy = mgr.GetModel("Copy");
            Assert.Equal("fv-guid-1", copy.PluginSettings.SelectedFieldViewIds["account"]);
            Assert.Equal("fv-guid-2", copy.PluginSettings.SelectedFieldViewIds["contact"]);
        }

        [Fact]
        public void CopyModel_PreservesAttributeDisplayNameOverrides()
        {
            var mgr = CreateManager();
            var src = MakeModel("Source");
            src.PluginSettings.AttributeDisplayNameOverrides = new Dictionary<string, Dictionary<string, string>>
            {
                ["incident"] = new Dictionary<string, string>
                {
                    ["statecode"] = "Service Status",
                    ["statuscode"] = "Ticket Status"
                }
            };
            mgr.CreateModel(src);

            mgr.CopyModel("Source", "Copy");

            var copy = mgr.GetModel("Copy");
            Assert.Equal("Service Status", copy.PluginSettings.AttributeDisplayNameOverrides["incident"]["statecode"]);
            Assert.Equal("Ticket Status", copy.PluginSettings.AttributeDisplayNameOverrides["incident"]["statuscode"]);
        }

        [Fact]
        public void CopyModel_PreservesFieldSelectionModes()
        {
            var mgr = CreateManager();
            var src = MakeModel("Source");
            src.PluginSettings.FieldSelectionModes = new Dictionary<string, string>
            {
                ["account"] = "Form",
                ["contact"] = "View",
                ["incident"] = "Custom"
            };
            mgr.CreateModel(src);

            mgr.CopyModel("Source", "Copy");

            var copy = mgr.GetModel("Copy");
            Assert.Equal("Form", copy.PluginSettings.FieldSelectionModes["account"]);
            Assert.Equal("View", copy.PluginSettings.FieldSelectionModes["contact"]);
            Assert.Equal("Custom", copy.PluginSettings.FieldSelectionModes["incident"]);
        }

        [Fact]
        public void CopyModel_PreservesTableIncludeCountMeasures()
        {
            var mgr = CreateManager();
            var src = MakeModel("Source");
            src.PluginSettings.TableIncludeCountMeasures = new Dictionary<string, bool>
            {
                ["account"] = true,
                ["contact"] = false
            };
            mgr.CreateModel(src);

            mgr.CopyModel("Source", "Copy");

            var copy = mgr.GetModel("Copy");
            Assert.True(copy.PluginSettings.TableIncludeCountMeasures["account"]);
            Assert.False(copy.PluginSettings.TableIncludeCountMeasures["contact"]);
        }

        [Fact]
        public void CopyModel_PreservesTableIncludeRecordLinkMeasures()
        {
            var mgr = CreateManager();
            var src = MakeModel("Source");
            src.PluginSettings.TableIncludeRecordLinkMeasures = new Dictionary<string, bool>
            {
                ["account"] = false,
                ["incident"] = true
            };
            mgr.CreateModel(src);

            mgr.CopyModel("Source", "Copy");

            var copy = mgr.GetModel("Copy");
            Assert.False(copy.PluginSettings.TableIncludeRecordLinkMeasures["account"]);
            Assert.True(copy.PluginSettings.TableIncludeRecordLinkMeasures["incident"]);
        }

        [Fact]
        public void CopyModel_PreservesAdditionalTableNames()
        {
            var mgr = CreateManager();
            var src = MakeModel("Source");
            src.PluginSettings.AdditionalTableNames = new List<string> { "product", "pricelevel" };
            mgr.CreateModel(src);

            mgr.CopyModel("Source", "Copy");

            var copy = mgr.GetModel("Copy");
            Assert.Contains("product", copy.PluginSettings.AdditionalTableNames);
            Assert.Contains("pricelevel", copy.PluginSettings.AdditionalTableNames);
        }

        [Fact]
        public void CopyModel_PreservesAdditionalRelationships()
        {
            var mgr = CreateManager();
            var src = MakeModel("Source");
            src.PluginSettings.AdditionalRelationships = new List<SerializedRelationship>
            {
                new SerializedRelationship
                {
                    SourceTable = "incident",
                    SourceAttribute = "productid",
                    TargetTable = "product",
                    IsActive = false,
                    IsSnowflake = false,
                    SnowflakeLevel = 0,
                    AssumeReferentialIntegrity = true
                }
            };
            mgr.CreateModel(src);

            mgr.CopyModel("Source", "Copy");

            var copy = mgr.GetModel("Copy");
            Assert.Single(copy.PluginSettings.AdditionalRelationships);
            var r = copy.PluginSettings.AdditionalRelationships[0];
            Assert.Equal("incident", r.SourceTable);
            Assert.Equal("productid", r.SourceAttribute);
            Assert.Equal("product", r.TargetTable);
            Assert.True(r.AssumeReferentialIntegrity);
        }

        [Fact]
        public void CopyModel_PreservesRelationshipSnowflakeLevelAndReferentialIntegrity()
        {
            var mgr = CreateManager();
            var src = MakeModel("Source");
            src.PluginSettings.Relationships = new List<SerializedRelationship>
            {
                new SerializedRelationship
                {
                    SourceTable = "account",
                    SourceAttribute = "accountcategorycode",
                    TargetTable = "category",
                    IsActive = true,
                    IsSnowflake = true,
                    SnowflakeLevel = 2,
                    AssumeReferentialIntegrity = true
                }
            };
            mgr.CreateModel(src);

            mgr.CopyModel("Source", "Copy");

            var copy = mgr.GetModel("Copy");
            Assert.Single(copy.PluginSettings.Relationships);
            var r = copy.PluginSettings.Relationships[0];
            Assert.Equal(2, r.SnowflakeLevel);
            Assert.True(r.AssumeReferentialIntegrity);
        }

        [Fact]
        public void CopyModel_PreservesLanguageCode()
        {
            var mgr = CreateManager();
            var src = MakeModel("Source");
            src.PluginSettings.LanguageCode = 1036; // French
            mgr.CreateModel(src);

            mgr.CopyModel("Source", "Copy");

            var copy = mgr.GetModel("Copy");
            Assert.Equal(1036, copy.PluginSettings.LanguageCode);
        }

        [Fact]
        public void CopyModel_PreservesDateTableConfig()
        {
            var mgr = CreateManager();
            var src = MakeModel("Source");
            src.PluginSettings.DateTableConfig = new DateTableConfig
            {
                PrimaryDateTable = "incident",
                PrimaryDateField = "createdon",
                TimeZoneId = "Eastern Standard Time",
                UtcOffsetHours = -5,
                StartYear = 2020,
                EndYear = 2030,
                WrappedFields = new List<DateTimeFieldConfig>
                {
                    new DateTimeFieldConfig
                    {
                        TableName = "incident",
                        FieldName = "modifiedon",
                        ConvertToDateOnly = false
                    }
                }
            };
            mgr.CreateModel(src);

            mgr.CopyModel("Source", "Copy");

            var copy = mgr.GetModel("Copy");
            var cfg = copy.PluginSettings.DateTableConfig;
            Assert.NotNull(cfg);
            Assert.Equal("incident", cfg!.PrimaryDateTable);
            Assert.Equal("createdon", cfg.PrimaryDateField);
            Assert.Equal("Eastern Standard Time", cfg.TimeZoneId);
            Assert.Equal(-5, cfg.UtcOffsetHours);
            Assert.Equal(2020, cfg.StartYear);
            Assert.Equal(2030, cfg.EndYear);
            Assert.Single(cfg.WrappedFields);
            Assert.Equal("incident", cfg.WrappedFields[0].TableName);
            Assert.Equal("modifiedon", cfg.WrappedFields[0].FieldName);
            Assert.False(cfg.WrappedFields[0].ConvertToDateOnly);
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private static SemanticModelConfig MakeModel(string name) => new SemanticModelConfig
        {
            Name = name,
            DataverseUrl = "https://test.crm.dynamics.com",
            WorkingFolder = @"C:\Temp\Model",
            PluginSettings = new PluginSettings()
        };
    }
}


