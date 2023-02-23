using CommunityToolkit.Mvvm.ComponentModel;
using SchildTeamsManager.Service.MicrosoftGraph;
using SchildTeamsManager.Settings;
using SchildTeamsManager.UI;
using SchildTeamsManager.UI.Dialog;
using System;
using System.Threading.Tasks;

namespace SchildTeamsManager.ViewModel
{
    public class SplashScreenViewModel : ObservableRecipient
    {
        private string progressText = string.Empty;

        public string ProgressText
        {
            get { return progressText; }
            set { SetProperty(ref progressText, value); }
        }

        #region Services

        private readonly IWindowManager windowManager;
        private readonly IMicrosoftGraph graph;
        private readonly ISettingsManager settingsManager;
        private readonly IDialogHelper dialogHelper;

        #endregion

        public SplashScreenViewModel(IMicrosoftGraph graph, IWindowManager windowManager, ISettingsManager settingsManager, IDialogHelper dialogHelper)
        {
            this.graph = graph;
            this.windowManager = windowManager; 
            this.settingsManager = settingsManager;
            this.dialogHelper = dialogHelper;
        }

        public async Task Authenticate()
        {
            try
            {
                ProgressText = "Einstellungen lesen...";
                await settingsManager.LoadSettingsAsync();

                if(!IsSettingsValid())
                {
                    windowManager.OpenSettingsView(true);
                    return;
                }

                ProgressText = "Bei Microsoft Graph anmelden...";
                await graph.AuthenticateAsync(settingsManager.Settings.Graph.TenantId, settingsManager.Settings.Graph.ClientId, settingsManager.Settings.Graph.ClientSecret);

                if (graph.IsAuthenticated)
                {
                    ProgressText = "Benutzer aus dem Azure Active Directory laden...";
                    await graph.LoadUsersAsync();

                    windowManager.OpenMainView();
                }
            }
            catch (Exception e)
            {
                dialogHelper.Show(new ErrorDialog
                {
                    Title = "Fehler",
                    Header = "Fehler beim Abrufen",
                    Content = "Beim Laden ist ein Fehler aufgetreten.",
                    Exception = e
                });

                windowManager.OpenSettingsView(true);
            }
        }

        private bool IsSettingsValid()
        {
            return !string.IsNullOrEmpty(settingsManager.Settings.SchILD.ConnectionString)
                && !string.IsNullOrEmpty(settingsManager.Settings.Graph.TenantId)
                && !string.IsNullOrEmpty(settingsManager.Settings.Graph.ClientId)
                && !string.IsNullOrEmpty(settingsManager.Settings.Graph.ClientSecret);
        }
    }
}
