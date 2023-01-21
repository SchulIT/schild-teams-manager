using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SchildTeamsManager.Settings
{
    public class JsonSettingsManager : ISettingsManager
    {
        public const string JsonFileName = "settings.json";
        public const string ApplicationName = "SchildTeamsManager";
        public const string ApplicationVendor = "SchulIT";
        
        protected static string SettingsDirectory
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    ApplicationVendor,
                    ApplicationName
                );
            }
        }

        protected static string SettingsJsonPath
        {
            get
            {
                return Path.Combine(
                    SettingsDirectory,
                    JsonFileName
                );
            }
        }

        public ISettings Settings { get; private set; }

        public async Task LoadSettingsAsync()
        {
            var directory = SettingsDirectory;
            var path = SettingsJsonPath;

            if(!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var settings = new JsonSettings();

            if(File.Exists(path))
            {
                using (var reader = new StreamReader(path))
                {
                    var json = await reader.ReadToEndAsync();
                    JsonConvert.PopulateObject(json, settings);
                }
            }

            Settings = settings;
        }

        public async Task SaveSettingsAsync()
        {
            using(var writer = new StreamWriter(SettingsJsonPath))
            {
                var json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
                await writer.WriteAsync(json);
            }
        }
    }
}
