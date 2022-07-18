using System.Windows;

namespace SchildTeamsManager.UI
{
    public interface IWindowManager
    {
        public void OpenMainView();

        public void OpenSettingsView(bool closeActiveWindow);

        public void CloseActiveWindow();

        public Window? GetActiveWindow();
    }
}
