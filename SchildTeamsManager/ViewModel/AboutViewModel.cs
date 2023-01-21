using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SchildTeamsManager.ViewModel
{
    public class AboutViewModel : ObservableRecipient
    {
        public string ApplicationName { get; private set; }

        public string Copyright { get; private set; }

        public string Version { get; private set; }

        public string ProjectUrl { get; } = @"https://github.com/SchulIT/schild-teams-manager"; // TODO: Read from assembly

        public List<Library> Libraries { get; } = new List<Library>();

        public AboutViewModel()
        {
            LoadInfo();
            LoadLibraries();
        }

        private void LoadInfo()
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            ApplicationName = GetName();
            Copyright = GetCopyright();
        }

        private string GetName()
        {
            var attribute = (Assembly.GetExecutingAssembly().GetCustomAttribute(typeof(AssemblyProductAttribute)) as AssemblyProductAttribute);

            if (attribute == null)
            {
                return null;
            }

            return attribute.Product;
        }

        private string GetCopyright()
        {
            var attribute = (Assembly.GetExecutingAssembly().GetCustomAttribute(typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute);

            if (attribute == null)
            {
                return null;
            }

            return attribute.Copyright;
        }

        public void LoadLibraries()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "SchildTeamsManager.licenses.json";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    var libraries = JsonConvert.DeserializeObject<List<Library>>(json);
                    Libraries.AddRange(libraries);
                }
            }
        }

        public class Library
        {
            public string PackageName { get; set; }

            public string PackageUrl { get; set; }

            public string Description { get; set; }

            public string LicenseUrl { get; set; }

            public string LicenseType { get; set; }
        }
    }
}
