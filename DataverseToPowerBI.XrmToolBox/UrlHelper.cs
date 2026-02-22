// =============================================================================
// UrlHelper.cs - Shared URL Utility Methods
// =============================================================================
// Purpose: Provides shared URL parsing utilities used across the plugin.
// Eliminates duplication between PluginControl and SemanticModelBuilder.
// =============================================================================

using System;

namespace DataverseToPowerBI.XrmToolBox
{
    /// <summary>
    /// Shared URL parsing utilities for Dataverse environment URLs.
    /// </summary>
    internal static class UrlHelper
    {
        /// <summary>
        /// Extracts the environment name from a Dataverse URL.
        /// </summary>
        /// <param name="dataverseUrl">
        /// The Dataverse environment URL (e.g., "https://myorg.crm.dynamics.com").
        /// </param>
        /// <returns>
        /// The environment name (e.g., "myorg"), or "default" if the URL is empty or unparseable.
        /// </returns>
        /// <example>
        /// UrlHelper.ExtractEnvironmentName("https://portfolioshapingdev.crm.dynamics.com")
        /// // returns "portfolioshapingdev"
        /// </example>
        public static string ExtractEnvironmentName(string dataverseUrl)
        {
            if (string.IsNullOrEmpty(dataverseUrl))
                return "default";

            // Remove protocol if present
            var url = dataverseUrl.Replace("https://", "").Replace("http://", "");

            // Get first segment before dot
            var firstDot = url.IndexOf('.');
            if (firstDot > 0)
                return url.Substring(0, firstDot);

            return url;
        }
    }
}
