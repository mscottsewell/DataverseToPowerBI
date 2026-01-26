using System;
using System.IO;
using Newtonsoft.Json;
using DataverseMetadataExtractor.Models;

namespace DataverseMetadataExtractor.Services
{
    public class SettingsManager
    {
        private const string SettingsFileName = ".dataverse_metadata_settings.json";
        private const string CacheFileName = ".dataverse_metadata_cache.json";
        private readonly string _settingsPath;
        private readonly string _cachePath;

        public SettingsManager()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "DataverseMetadataExtractor");
            Directory.CreateDirectory(appFolder);
            
            _settingsPath = Path.Combine(appFolder, SettingsFileName);
            _cachePath = Path.Combine(appFolder, CacheFileName);
        }

        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch
            {
                // If loading fails, return default settings
            }
            
            return new AppSettings();
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save settings: {ex.Message}", ex);
            }
        }

        public MetadataCache? LoadCache()
        {
            try
            {
                if (File.Exists(_cachePath))
                {
                    var json = File.ReadAllText(_cachePath);
                    return JsonConvert.DeserializeObject<MetadataCache>(json);
                }
            }
            catch
            {
                // If loading fails, return null
            }
            
            return null;
        }

        public void SaveCache(MetadataCache cache)
        {
            try
            {
                var json = JsonConvert.SerializeObject(cache, Formatting.Indented);
                File.WriteAllText(_cachePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save cache: {ex.Message}", ex);
            }
        }

        public void ClearCache()
        {
            try
            {
                if (File.Exists(_cachePath))
                {
                    File.Delete(_cachePath);
                }
            }
            catch
            {
                // Ignore errors when clearing cache
            }
        }
    }
}
