// ===================================================================================
// RibbonIcons.cs - Icon Resources for XrmToolBox Toolbar
// ===================================================================================
//
// PURPOSE:
// Provides 20x20 pixel icons for the XrmToolBox plugin toolbar buttons
// loaded from embedded PNG resources.
//
// ICONS PROVIDED:
// - FolderIcon: Yellow folder for "Change Working Folder"
// - TableIcon: Blue grid for "Select Tables"
// - BuildIcon: Gear for "Build Model"
// - CalendarIcon: Calendar grid for "Configure Date Table"
// - SettingsIcon: Gray gear for settings
// - RefreshIcon: Circular arrows for "Sync/Refresh"
// - ModelIcon: Database for "Semantic Model Selector"
//
// The PNG icons are stored in Assets/Icons/ and embedded as resources
//
// ===================================================================================

using System.Drawing;
using System.IO;
using System.Reflection;

namespace DataverseToPowerBI.XrmToolBox
{
    /// <summary>
    /// Provides icons for the ribbon toolbar buttons from embedded resources
    /// </summary>
    public static class RibbonIcons
    {
        private static readonly Assembly _assembly = typeof(RibbonIcons).Assembly;
        private const string ResourcePrefix = "DataverseToPowerBI.XrmToolBox.Assets.Icons.";

        private static Image LoadIcon(string iconName)
        {
            var resourceName = $"{ResourcePrefix}{iconName}.png";
            using (var stream = _assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    return CreatePlaceholder();
                return Image.FromStream(stream);
            }
        }

        private static Image CreatePlaceholder()
        {
            // Fallback: simple 20x20 gray square if resource not found
            var bmp = new Bitmap(20, 20);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.LightGray);
                g.DrawRectangle(Pens.Gray, 0, 0, 19, 19);
            }
            return bmp;
        }

        /// <summary>
        /// Folder icon for Change Working Folder
        /// </summary>
        public static Image FolderIcon => LoadIcon("FolderIcon");

        /// <summary>
        /// Table/grid icon for Select Tables
        /// </summary>
        public static Image TableIcon => LoadIcon("TableIcon");

        /// <summary>
        /// Build/gear icon for Build Semantic Model
        /// </summary>
        public static Image BuildIcon => LoadIcon("BuildIcon");

        /// <summary>
        /// Calendar icon for Calendar Table feature
        /// </summary>
        public static Image CalendarIcon => LoadIcon("CalendarIcon");

        /// <summary>
        /// Settings/gear icon
        /// </summary>
        public static Image SettingsIcon => LoadIcon("SettingsIcon");

        /// <summary>
        /// Refresh/reload icon with circular arrow
        /// </summary>
        public static Image RefreshIcon => LoadIcon("RefreshIcon");

        /// <summary>
        /// Database/model icon for semantic model selector
        /// </summary>
        public static Image ModelIcon => LoadIcon("ModelIcon");

        /// <summary>
        /// Preview icon for TMDL Preview
        /// </summary>
        public static Image PreviewIcon => LoadIcon("TMDLPreviewIcon");
    }
}
