using System;
using System.Reflection;
using DataverseToPowerBI.Core.Models;
using DataverseToPowerBI.XrmToolBox;
using Xunit;

namespace DataverseToPowerBI.Tests
{
    public class ExpandLookupFormTests
    {
        private static readonly MethodInfo IsExcludedAttributeMethod = typeof(ExpandLookupForm)
            .GetMethod("IsExcludedAttribute", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Could not find ExpandLookupForm.IsExcludedAttribute");

        [Theory]
        [InlineData("statecode")]
        [InlineData("statuscode")]
        public void IsExcludedAttribute_StateAndStatusAreNotExcluded(string logicalName)
        {
            var attribute = new AttributeMetadata
            {
                LogicalName = logicalName,
                DisplayName = logicalName,
                AttributeType = logicalName == "statecode" ? "State" : "Status"
            };

            var isExcluded = (bool)IsExcludedAttributeMethod.Invoke(null, new object[] { attribute, "accountid" });

            Assert.False(isExcluded);
        }
    }
}