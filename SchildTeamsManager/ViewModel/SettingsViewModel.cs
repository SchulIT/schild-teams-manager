using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using SchildTeamsManager.Service.MicrosoftGraph;
using SchildTeamsManager.Settings;
using SchildTeamsManager.UI.Dialog;
using SchildTeamsManager.ViewModel.Form;
using SchulIT.SchildExport;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SchildTeamsManager.ViewModel
{
    public class SettingsViewModel : ObservableRecipient
    {
        #region Properties

        private string busyText;

        public string BusyText
        {
            get { return busyText; }
            set { SetProperty(ref busyText, value); }
        }

        private bool isBusy = false;

        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        private bool isSuccessfulConfig = false;

        public bool IsSuccessfulConfig
        {
            get { return isSuccessfulConfig; }
            set
            {
                SetProperty(ref isSuccessfulConfig, value);
                SaveSettingsCommand?.NotifyCanExecuteChanged();
            }
        }

        public SettingsForm Settings { get; } = new SettingsForm();

        #endregion

        #region Commands

        public RelayCommand LoadSettingsCommand { get; private set; }

        public AsyncRelayCommand TestConnectionCommand { get; private set; }

        public AsyncRelayCommand SaveSettingsCommand { get; private set; }

        #endregion

        #region Services

        private readonly IMicrosoftGraph graph;
        private readonly IExporter schildExporter;
        private readonly ISettingsManager settingsManager;
        private readonly IDialogHelper dialogHelper;

        #endregion

        public SettingsViewModel(IMicrosoftGraph graph, IExporter exporter, ISettingsManager settingsManager, IDialogHelper dialogHelper)
        {
            this.graph = graph;
            this.schildExporter = exporter;
            this.settingsManager = settingsManager;
            this.dialogHelper = dialogHelper;

            LoadSettingsCommand = new RelayCommand(LoadSettings);
            TestConnectionCommand = new AsyncRelayCommand(TestConnectionAsync, CanTest);
            SaveSettingsCommand = new AsyncRelayCommand(SaveAsync, CanSave);

            Settings.ErrorsChanged += delegate
            {
                TestConnectionCommand?.NotifyCanExecuteChanged();
            };
        }

        private void LoadSettings()
        {
            Settings.ConnectionString = settingsManager.Settings.SchILD.ConnectionString;
            Settings.OnlyVisible = settingsManager.Settings.SchILD.OnlyVisibleEntities;

            Settings.TenantId = settingsManager.Settings.Graph.TenantId;
            Settings.ClientId = settingsManager.Settings.Graph.ClientId;
            Settings.ClientSecret = settingsManager.Settings.Graph.ClientSecret;
        }

        private async Task TestConnectionAsync()
        {
            try
            {
                IsSuccessfulConfig = false;
                IsBusy = true;
                BusyText = "Verbinde zu Microsoft Graph...";
                await graph.AuthenticateAsync(Settings.TenantId, Settings.ClientId, Settings.ClientSecret);

                BusyText = "Verbinde zur SchILD-Datenbank...";
                schildExporter.Configure(Settings.ConnectionString, false);
                await schildExporter.GetSchoolInfoAsync();
                IsSuccessfulConfig = true;
            }
            catch (Exception e)
            {
                dialogHelper.Show(new ErrorDialog
                {
                    Title = "Fehler",
                    Header = "Fehler beim Verbinden",
                    Content = "Beim Testen der Verbindung ist es zu einem Fehler gekommen.",
                    Exception = e
                });
            }
            finally
            {
                IsBusy = false;
                BusyText = string.Empty;
            }
        }

        private bool CanTest() => Settings.HasErrors == false;

        private bool CanSave() => IsSuccessfulConfig;

        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;
                BusyText = "Einstellungen speichern";

                settingsManager.Settings.SchILD.ConnectionString = Settings.ConnectionString;
                settingsManager.Settings.SchILD.OnlyVisibleEntities = Settings.OnlyVisible;
                settingsManager.Settings.Graph.TenantId = Settings.TenantId;
                settingsManager.Settings.Graph.ClientId = Settings.ClientId;
                settingsManager.Settings.Graph.ClientSecret = Settings.ClientSecret;

                await settingsManager.SaveSettingsAsync();
            }
            catch (Exception e)
            {
                dialogHelper.Show(new ErrorDialog
                {
                    Title = "Fehler",
                    Header = "Fehler beim Speichern",
                    Content = "Beim Speichern der Einstellungen ist es zu einem Fehler gekommen.",
                    Exception = e
                });
            }
            finally
            {
                IsBusy = false;
                BusyText = null;
            }
        }

        private bool CanSaveSettings() => !IsBusy && IsSuccessfulConfig;

        
    }
}
