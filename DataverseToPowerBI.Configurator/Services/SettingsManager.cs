using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using DataverseToPowerBI.Configurator.Models;

namespace DataverseToPowerBI.Configurator.Services
{
    public class SettingsManager
    {
        private const string ConfigurationsFileName = ".dataverse_configurations.json";
        private readonly string _configurationsPath;
        private readonly string _appFolder;
        private ConfigurationsFile? _configurationsFile;

        public SettingsManager()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _appFolder = Path.Combine(appDataPath, "DataverseToPowerBI.Configurator");
            Directory.CreateDirectory(_appFolder);
            
            _configurationsPath = Path.Combine(_appFolder, ConfigurationsFileName);
        }

        private string GetCachePath(string configurationName)
        {
            // Sanitize configuration name for use in filename
            var sanitized = string.Join("_", configurationName.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(_appFolder, $".dataverse_metadata_cache_{sanitized}.json");
        }

        private ConfigurationsFile LoadConfigurationsFile()
        {
            try
            {
                if (File.Exists(_configurationsPath))
                {
                    var json = File.ReadAllText(_configurationsPath);
                    return JsonConvert.DeserializeObject<ConfigurationsFile>(json) ?? new ConfigurationsFile();
                }
            }
            catch
            {
                // If loading fails, return new file
            }
            
            return new ConfigurationsFile();
        }

        private void SaveConfigurationsFile(ConfigurationsFile configurationsFile)
        {
            try
            {
                // DEBUG: Check what's being serialized
                var currentConfig = configurationsFile.Configurations
                    .FirstOrDefault(c => c.Name == configurationsFile.LastUsedConfigurationName);
                var attrInfoCount = currentConfig?.Settings?.AttributeDisplayInfo?.Sum(t => t.Value.Count) ?? 0;
                DebugLogger.LogSection("SaveConfigurationsFile - Before JSON Write",
                    $"Config: {configurationsFile.LastUsedConfigurationName}\n" +
                    $"AttributeDisplayInfo: {attrInfoCount} attrs across {currentConfig?.Settings?.AttributeDisplayInfo?.Count ?? 0} tables\n" +
                    $"File: {_configurationsPath}");
                
                var json = JsonConvert.SerializeObject(configurationsFile, Formatting.Indented);
                File.WriteAllText(_configurationsPath, json);
                _configurationsFile = configurationsFile;
                
                // DEBUG: Verify what was written
                var verifyJson = File.ReadAllText(_configurationsPath);
                var verifyFile = JsonConvert.DeserializeObject<ConfigurationsFile>(verifyJson);
                var verifyConfig = verifyFile?.Configurations
                    .FirstOrDefault(c => c.Name == configurationsFile.LastUsedConfigurationName);
                var verifyAttrCount = verifyConfig?.Settings?.AttributeDisplayInfo?.Sum(t => t.Value.Count) ?? 0;
                DebugLogger.LogSection("SaveConfigurationsFile - After JSON Write (Verified)",
                    $"AttributeDisplayInfo: {verifyAttrCount} attrs across {verifyConfig?.Settings?.AttributeDisplayInfo?.Count ?? 0} tables");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save configurations: {ex.Message}", ex);
            }
        }

        public AppSettings LoadSettings()
        {
            _configurationsFile = LoadConfigurationsFile();
            
            // If there are no configurations, create a default one
            if (_configurationsFile.Configurations.Count == 0)
            {
                var defaultConfig = new ConfigurationEntry
                {
                    Name = "Default",
                    LastUsed = DateTime.Now,
                    Settings = new AppSettings()
                };
                _configurationsFile.Configurations.Add(defaultConfig);
                _configurationsFile.LastUsedConfigurationName = "Default";
                SaveConfigurationsFile(_configurationsFile);
                return defaultConfig.Settings;
            }

            // Load the last used configuration
            var lastUsed = _configurationsFile.Configurations
                .FirstOrDefault(c => c.Name == _configurationsFile.LastUsedConfigurationName)
                ?? _configurationsFile.Configurations
                    .OrderByDescending(c => c.LastUsed)
                    .First();

            return lastUsed.Settings;
        }

        public void SaveSettings(AppSettings settings)
        {
            if (_configurationsFile == null)
            {
                _configurationsFile = LoadConfigurationsFile();
            }

            var currentConfig = _configurationsFile.Configurations
                .FirstOrDefault(c => c.Name == _configurationsFile.LastUsedConfigurationName);

            if (currentConfig != null)
            {
                currentConfig.Settings = settings;
                currentConfig.LastUsed = DateTime.Now;
            }
            else
            {
                // Create a new configuration if current one doesn't exist
                var newConfig = new ConfigurationEntry
                {
                    Name = "Default",
                    LastUsed = DateTime.Now,
                    Settings = settings
                };
                _configurationsFile.Configurations.Add(newConfig);
                _configurationsFile.LastUsedConfigurationName = "Default";
            }

            SaveConfigurationsFile(_configurationsFile);
        }

        public string GetCurrentConfigurationName()
        {
            if (_configurationsFile == null)
            {
                _configurationsFile = LoadConfigurationsFile();
            }
            return _configurationsFile.LastUsedConfigurationName ?? "Default";
        }

        public List<string> GetConfigurationNames()
        {
            if (_configurationsFile == null)
            {
                _configurationsFile = LoadConfigurationsFile();
            }
            return _configurationsFile.Configurations.Select(c => c.Name).ToList();
        }

        public void SwitchToConfiguration(string configurationName)
        {
            if (_configurationsFile == null)
            {
                _configurationsFile = LoadConfigurationsFile();
            }

            var config = _configurationsFile.Configurations.FirstOrDefault(c => c.Name == configurationName);
            if (config == null)
            {
                throw new Exception($"Configuration '{configurationName}' not found.");
            }

            config.LastUsed = DateTime.Now;
            _configurationsFile.LastUsedConfigurationName = configurationName;
            SaveConfigurationsFile(_configurationsFile);
        }

        public AppSettings GetConfiguration(string configurationName)
        {
            if (_configurationsFile == null)
            {
                _configurationsFile = LoadConfigurationsFile();
            }

            var config = _configurationsFile.Configurations.FirstOrDefault(c => c.Name == configurationName);
            if (config == null)
            {
                throw new Exception($"Configuration '{configurationName}' not found.");
            }

            return config.Settings;
        }

        public void CreateNewConfiguration(string configurationName, AppSettings? settings = null)
        {
            if (_configurationsFile == null)
            {
                _configurationsFile = LoadConfigurationsFile();
            }

            if (_configurationsFile.Configurations.Any(c => c.Name == configurationName))
            {
                throw new Exception($"Configuration '{configurationName}' already exists.");
            }

            var newConfig = new ConfigurationEntry
            {
                Name = configurationName,
                LastUsed = DateTime.Now,
                Settings = settings ?? new AppSettings()
            };

            _configurationsFile.Configurations.Add(newConfig);
            _configurationsFile.LastUsedConfigurationName = configurationName;
            SaveConfigurationsFile(_configurationsFile);
        }

        public void RenameConfiguration(string oldName, string newName)
        {
            if (_configurationsFile == null)
            {
                _configurationsFile = LoadConfigurationsFile();
            }

            var config = _configurationsFile.Configurations.FirstOrDefault(c => c.Name == oldName);
            if (config == null)
            {
                throw new Exception($"Configuration '{oldName}' not found.");
            }

            if (_configurationsFile.Configurations.Any(c => c.Name == newName))
            {
                throw new Exception($"Configuration '{newName}' already exists.");
            }

            config.Name = newName;
            if (_configurationsFile.LastUsedConfigurationName == oldName)
            {
                _configurationsFile.LastUsedConfigurationName = newName;
            }

            SaveConfigurationsFile(_configurationsFile);
        }

        public void DeleteConfiguration(string configurationName)
        {
            if (_configurationsFile == null)
            {
                _configurationsFile = LoadConfigurationsFile();
            }

            var config = _configurationsFile.Configurations.FirstOrDefault(c => c.Name == configurationName);
            if (config == null)
            {
                throw new Exception($"Configuration '{configurationName}' not found.");
            }

            if (_configurationsFile.Configurations.Count == 1)
            {
                throw new Exception("Cannot delete the last configuration.");
            }

            _configurationsFile.Configurations.Remove(config);

            // If we deleted the current configuration, switch to the most recent one
            if (_configurationsFile.LastUsedConfigurationName == configurationName)
            {
                var mostRecent = _configurationsFile.Configurations
                    .OrderByDescending(c => c.LastUsed)
                    .First();
                _configurationsFile.LastUsedConfigurationName = mostRecent.Name;
            }

            SaveConfigurationsFile(_configurationsFile);
        }

        public string GetSettingsFolderPath()
        {
            return _appFolder;
        }

        public string? GetMostRecentConfigurationForEnvironment(string environmentUrl)
        {
            if (_configurationsFile == null)
            {
                _configurationsFile = LoadConfigurationsFile();
            }

            var matchingConfigs = _configurationsFile.Configurations
                .Where(c => c.Settings.LastEnvironmentUrl?.Equals(environmentUrl, StringComparison.OrdinalIgnoreCase) == true)
                .OrderByDescending(c => c.LastUsed)
                .ToList();

            return matchingConfigs.FirstOrDefault()?.Name;
        }

        public MetadataCache? LoadCache(string? configurationName = null)
        {
            try
            {
                var cachePath = GetCachePath(configurationName ?? GetCurrentConfigurationName());
                if (File.Exists(cachePath))
                {
                    var json = File.ReadAllText(cachePath);
                    return JsonConvert.DeserializeObject<MetadataCache>(json);
                }
            }
            catch
            {
                // If loading fails, return null
            }
            
            return null;
        }

        public void SaveCache(MetadataCache cache, string? configurationName = null)
        {
            try
            {
                var cachePath = GetCachePath(configurationName ?? GetCurrentConfigurationName());
                var json = JsonConvert.SerializeObject(cache, Formatting.Indented);
                File.WriteAllText(cachePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save cache: {ex.Message}", ex);
            }
        }

        public void ClearCache(string? configurationName = null)
        {
            try
            {
                var cachePath = GetCachePath(configurationName ?? GetCurrentConfigurationName());
                if (File.Exists(cachePath))
                {
                    File.Delete(cachePath);
                }
            }
            catch
            {
                // Ignore errors when clearing cache
            }
        }

        /// <summary>
        /// Removes cache files for configurations that no longer exist
        /// </summary>
        public int CleanupOrphanedCacheFiles()
        {
            try
            {
                if (_configurationsFile == null)
                {
                    _configurationsFile = LoadConfigurationsFile();
                }

                var validConfigNames = _configurationsFile.Configurations
                    .Select(c => c.Name)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var cacheFiles = Directory.GetFiles(_appFolder, ".dataverse_metadata_cache_*.json");
                int removedCount = 0;

                foreach (var cacheFile in cacheFiles)
                {
                    var fileName = Path.GetFileName(cacheFile);
                    // Extract config name from: .dataverse_metadata_cache_{configname}.json
                    var configName = fileName
                        .Replace(".dataverse_metadata_cache_", "")
                        .Replace(".json", "")
                        .Replace("_", " ");  // Unsanitize the name

                    if (!validConfigNames.Contains(configName))
                    {
                        DebugLogger.Log($"Cleaning up orphaned cache file: {fileName} (config '{configName}' not found)");
                        File.Delete(cacheFile);
                        removedCount++;
                    }
                }

                return removedCount;
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"Error cleaning up orphaned cache files: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Gets diagnostic information about the settings storage
        /// </summary>
        public string GetSettingsDiagnostics()
        {
            try
            {
                if (_configurationsFile == null)
                {
                    _configurationsFile = LoadConfigurationsFile();
                }

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("=== Settings Storage Diagnostics ===");
                sb.AppendLine();
                sb.AppendLine($"Settings Folder: {_appFolder}");
                sb.AppendLine($"Configurations File: {_configurationsPath}");
                sb.AppendLine($"File Exists: {File.Exists(_configurationsPath)}");
                sb.AppendLine();
                sb.AppendLine($"Total Configurations: {_configurationsFile.Configurations.Count}");
                sb.AppendLine($"Last Used Configuration: {_configurationsFile.LastUsedConfigurationName}");
                sb.AppendLine();

                foreach (var config in _configurationsFile.Configurations)
                {
                    sb.AppendLine($"Configuration: {config.Name}");
                    sb.AppendLine($"  Last Used: {config.LastUsed:g}");
                    sb.AppendLine($"  Environment URL: {config.Settings.LastEnvironmentUrl ?? "(not set)"}");
                    sb.AppendLine($"  Solution: {config.Settings.LastSolution ?? "(not set)"}");
                    sb.AppendLine($"  Project Name: {config.Settings.ProjectName ?? "(not set)"}");
                    sb.AppendLine($"  Output Folder: {config.Settings.OutputFolder ?? "(not set)"}");
                    sb.AppendLine($"  Selected Tables: {config.Settings.SelectedTables.Count}");
                    
                    var cachePath = GetCachePath(config.Name);
                    sb.AppendLine($"  Cache File: {Path.GetFileName(cachePath)}");
                    sb.AppendLine($"  Cache Exists: {File.Exists(cachePath)}");
                    if (File.Exists(cachePath))
                    {
                        var fileInfo = new FileInfo(cachePath);
                        sb.AppendLine($"  Cache Size: {fileInfo.Length:N0} bytes");
                        sb.AppendLine($"  Cache Modified: {fileInfo.LastWriteTime:g}");
                    }
                    sb.AppendLine();
                }

                // Check for orphaned cache files
                var cacheFiles = Directory.GetFiles(_appFolder, ".dataverse_metadata_cache_*.json");
                var validConfigNames = _configurationsFile.Configurations
                    .Select(c => c.Name)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var orphanedFiles = new List<string>();
                foreach (var cacheFile in cacheFiles)
                {
                    var fileName = Path.GetFileName(cacheFile);
                    var configName = fileName
                        .Replace(".dataverse_metadata_cache_", "")
                        .Replace(".json", "")
                        .Replace("_", " ");

                    if (!validConfigNames.Contains(configName))
                    {
                        orphanedFiles.Add(fileName);
                    }
                }

                if (orphanedFiles.Any())
                {
                    sb.AppendLine("⚠️ Orphaned Cache Files (no matching configuration):");
                    foreach (var file in orphanedFiles)
                    {
                        sb.AppendLine($"  - {file}");
                    }
                }
                else
                {
                    sb.AppendLine("✅ No orphaned cache files found");
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"Error generating diagnostics: {ex.Message}";
            }
        }
    }
}
