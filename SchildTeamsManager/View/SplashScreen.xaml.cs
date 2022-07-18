using SchildTeamsManager.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace SchildTeamsManager.View
{
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();

            MouseDown += OnMouseDown;
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as SplashScreenViewModel;
            if (viewModel == null)
            {
                return;
            }

            try
            {
                await viewModel.Authenticate();
            }
            catch
            {

            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
