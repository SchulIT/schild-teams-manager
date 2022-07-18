using SchildTeamsManager.View;
using System.Linq;
using System.Windows;

namespace SchildTeamsManager.UI
{
    public class WindowManager : IWindowManager
    {
        public void CloseActiveWindow()
        {
            GetActiveWindow()?.Close();
        }

        public void OpenMainView()
        {
            var currentWindow = GetActiveWindow();

            var view = new MainView();
            view.Show();

            currentWindow?.Close();
        }

        public void OpenSettingsView(bool closeActiveWindow)
        {
            var currentWindow = GetActiveWindow();

            var view = new SettingsView();

            if (closeActiveWindow)
            {
                view.Show();
                currentWindow?.Close();
            }
            else
            {
                view.Owner = currentWindow;
                view.ShowDialog();
            }
        }

        public Window? GetActiveWindow()
        {
            return Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
        }
    }
}
