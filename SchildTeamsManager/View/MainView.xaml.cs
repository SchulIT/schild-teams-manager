using ModernWpf.Controls;
using System;
using System.Linq;
using System.Windows;

namespace SchildTeamsManager.View
{
    /// <summary>
    /// Interaktionslogik für MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();

            navigationView.SelectedItem = navigationView.MenuItems.OfType<NavigationViewItem>().First();
        }

        private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = args.SelectedItem as NavigationViewItem;
            if (selectedItem == null)
            {
                return;
            }

            Type? targetPage = selectedItem.Tag switch
            {
                "teams" => typeof(TeamsPage),
                "grades" => typeof(GradesPage),
                "tuitions" => typeof(TuitionsPage),
                "settings" => typeof(SettingsPage),
                _ => null,
            };

            if (targetPage != null)
            {
                frame.Navigate(targetPage);
            }
        }
    }
}
