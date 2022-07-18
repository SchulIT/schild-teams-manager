using ModernWpf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows.Media;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SchildTeamsManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ThemeManager.Current.AccentColor = (Color)ColorConverter.ConvertFromString("#6B69D6");
        }
    }
}
