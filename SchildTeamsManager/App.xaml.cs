using ModernWpf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows.Media;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LinqToDB.Common;

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
            // related to https://github.com/linq2db/linq2db/issues/365
            //LinqToDB.Common.Configuration.Linq.GuardGrouping = false;
        }
    }
}
