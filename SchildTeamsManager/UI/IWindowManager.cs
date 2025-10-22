using System.Windows;

namespace SchildTeamsManager.UI
{
    public interface IWindowManager
    {
        public void OpenMainView();

        public void OpenSettingsView();

        public Window? GetFirstOpenedWindow();
    }
}
