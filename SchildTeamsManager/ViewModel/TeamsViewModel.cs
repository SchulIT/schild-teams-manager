using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchildTeamsManager.Model;
using SchildTeamsManager.Service.MicrosoftGraph;
using SchildTeamsManager.Settings;
using SchildTeamsManager.UI.Dialog;
using SchulIT.SchildExport;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SchildTeamsManager.ViewModel
{
    public class TeamsViewModel : ObservableRecipient
    {
        private bool isLoading;

        public bool IsLoading
        {
            get { return isLoading; }
            set { SetProperty(ref isLoading, value); }
        }

        private bool isHideArchivedTeamsEnabled;

        public bool IsHideArchivedTeamsEnabled
        {
            get { return isHideArchivedTeamsEnabled; }
            set
            {
                SetProperty(ref isHideArchivedTeamsEnabled, value);
                ApplyFilter();
            }
        }

        public List<int> SchoolYears { get; } = Enumerable.Range(2018, DateTime.Now.Year - 2018 + 1).ToList();

        private int schoolYear;

        public int SchoolYear
        {
            get { return schoolYear; }
            set
            {
                SetProperty(ref schoolYear, value);
                ApplyFilter();
            }
        }

        private string filterQuery = string.Empty;

        public string FilterQuery
        {
            get { return filterQuery; }
            set
            {
                SetProperty(ref filterQuery, value);
                ApplyFilter();
            }
        }

        private ObservableCollection<Team> Teams { get; } = new ObservableCollection<Team>();

        public ICollectionView TeamsView { get; }

        public ObservableCollection<Team> SelectedTeams { get; } = new ObservableCollection<Team>();

        #region Command

        public AsyncRelayCommand LoadCurrentSchoolYearCommand { get; private set; }
        public AsyncRelayCommand LoadTeamsCommand { get; private set; }

        public AsyncRelayCommand ArchiveTeamsCommand { get; private set; }

        public AsyncRelayCommand UnarchiveTeamsCommand { get; private set; }

        public AsyncRelayCommand RemoveTeamsCommand { get; private set; }

        #endregion

        #region Services

        private readonly IMicrosoftGraph graph;
        private readonly IExporter schildExporter;
        private readonly ISettingsManager settingsManager;
        private readonly IDialogHelper dialogHelper;

        #endregion

        public TeamsViewModel(IMicrosoftGraph graph, IExporter schildExporter, ISettingsManager settingsManager, IDialogHelper dialogHelper)
        {
            this.graph = graph;
            this.schildExporter = schildExporter;
            this.settingsManager = settingsManager;
            this.dialogHelper = dialogHelper;
            LoadCurrentSchoolYearCommand = new AsyncRelayCommand(LoadCurrentSchoolYear);
            LoadTeamsCommand = new AsyncRelayCommand(LoadTeams);
            ArchiveTeamsCommand = new AsyncRelayCommand(ArchiveTeams, CanArchiveTeams);
            UnarchiveTeamsCommand = new AsyncRelayCommand(UnarchiveTeams, CanUnarchiveTeams);
            RemoveTeamsCommand = new AsyncRelayCommand(RemoveTeams, CanRemoveTeams);

            TeamsView = CollectionViewSource.GetDefaultView(Teams);
            SelectedTeams.CollectionChanged += delegate
            {
                ArchiveTeamsCommand?.NotifyCanExecuteChanged();
                UnarchiveTeamsCommand?.NotifyCanExecuteChanged();
                RemoveTeamsCommand?.NotifyCanExecuteChanged();
            };
        }

        private async Task LoadCurrentSchoolYear()
        {
            if (settingsManager.Settings == null)
            {
                dialogHelper.Show(new Dialog { Title = "Fehler", Header = "Einstellungen nicht geladen", Content = "Die Anwendungseinstellungen wurden nicht ordnungsgemäß geladen. Bitte das Programm neu starten und erneut probieren." });
                return;
            }

            if (string.IsNullOrEmpty(settingsManager.Settings.SchILD.ConnectionString))
            {
                dialogHelper.Show(new Dialog { Title = "Fehler", Header = "SchILD-Einstellungen unvollständig", Content = "Bitte die Verbindungszeichenfolge für SchILD in den Einstellungen konfigurieren." });
                return;
            }

            schildExporter.Configure(settingsManager.Settings.SchILD.ConnectionString, false);

            var schoolInfo = await schildExporter.GetSchoolInfoAsync();

            if (schoolInfo.CurrentYear != null)
            {
                SchoolYear = schoolInfo.CurrentYear.Value;
            }
        }

        private async Task ArchiveTeams()
        {
            try
            {
                IsLoading = true;
                var batchSize = 50;

                for (int idx = 0; idx < Math.Ceiling((double)SelectedTeams.Count / batchSize); idx++)
                {
                    var offset = idx * batchSize;
                    var tasks = new List<Task>();
                    foreach (var team in SelectedTeams.Skip(offset).Take(batchSize))
                    {
                        if (team.IsArchived != true)
                        {
                            tasks.Add(graph.ArchiveTeam(team));
                        }
                    }

                    await Task.WhenAll(tasks);

                    foreach (var team in SelectedTeams.Skip(offset).Take(batchSize))
                    {
                        team.IsArchived = true;
                    }
                }
            }
            catch (Exception ex)
            {
                dialogHelper.Show(new ErrorDialog { Title = "Fehler", Header = "Fehler beim Archivieren des Teams", Content = "Bitte die Details anschauen zwecks Fehlerursache.", Exception = ex });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanArchiveTeams() => SelectedTeams.Count > 0;

        private async Task UnarchiveTeams()
        {
            try
            {
                IsLoading = true;
                var batchSize = 50;

                for (int idx = 0; idx < Math.Ceiling((double)SelectedTeams.Count / batchSize); idx++)
                {
                    var offset = idx * batchSize;
                    var tasks = new List<Task>();
                    foreach (var team in SelectedTeams.Skip(offset).Take(batchSize))
                    {
                        if (team.IsArchived != true)
                        {
                            tasks.Add(graph.UnarchiveTeam(team));
                        }
                    }

                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception ex)
            {
                dialogHelper.Show(new ErrorDialog { Title = "Fehler", Header = "Fehler beim Aufheben der Archivierung eines Teams", Content = "Bitte die Details anschauen zwecks Fehlerursache.", Exception = ex });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanUnarchiveTeams() => SelectedTeams.Count > 0;

        private async Task LoadTeams()
        {
            try
            {
                IsLoading = true;
                var teams = await graph.GetTeamsAsync(SchoolYear);

                Teams.Clear();
                var teamsDictionary = new Dictionary<string, Team>();
                foreach (var team in teams)
                {
                    Teams.Add(team);
                    teamsDictionary.Add(team.GroupId, team);
                }

                // Check IsArchived-Status
                var tasks = new List<Task<Dictionary<string, bool?>>>();
                var batchSize = 20;
                for(int idx = 0; idx < Math.Ceiling((double)Teams.Count / batchSize); idx++)
                {
                    var offset = idx * batchSize;
                    var teamsBatch = Teams.Skip(offset).Take(batchSize);
                    tasks.Add(graph.GetTeamsArchiveStatus(teamsBatch));
                }

                await Task.WhenAll(tasks);

                foreach(var task in tasks)
                {
                    foreach(var kv in task.Result)
                    {
                        if(teamsDictionary.ContainsKey(kv.Key))
                        {
                            teamsDictionary[kv.Key].IsArchived = kv.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                dialogHelper.Show(new ErrorDialog { Title = "Fehler", Header = "Fehler beim Laden der Teams", Content = "Bitte die Details anschauen zwecks Fehlerursache.", Exception = ex });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RemoveTeams()
        {
            dialogHelper.Show(
                new ConfirmDialog
                {
                    Title = "Löschen bestätigen",
                    Header = "Löschen bestätigen",
                    Content = $"Es wurden {SelectedTeams.Count} Team(s) zum Löschen ausgewählt. Sollen die Teams wirklich gelöscht werden?",
                    ConfirmAction = async () =>
                    {
                        try
                        {
                            IsLoading = true;

                            var selectedItems = SelectedTeams.ToArray();
                            var batchSize = 50;
                            var batches = Math.Ceiling((double)selectedItems.Length / batchSize);

                            for (int idx = 0; idx < batches; idx++)
                            {
                                var offset = idx * batchSize;
                                var tasks = new List<Task>();
                                foreach (var team in selectedItems.Skip(offset).Take(batchSize))
                                {
                                    tasks.Add(graph.RemoveTeamAsync(team));
                                }

                                await Task.WhenAll(tasks);

                                foreach (var team in selectedItems.Skip(offset).Take(batchSize))
                                {
                                    Teams.Remove(team);
                                }
                            }

                            TeamsView.Refresh();
                            SelectedTeams.Clear();
                        }
                        catch (Exception ex)
                        {
                            dialogHelper.Show(new ErrorDialog { Title = "Fehler", Header = "Fehler beim Löschen der Teams", Content = "Bitte die Details anschauen zwecks Fehlerursache.", Exception = ex });
                        }
                        finally
                        {
                            IsLoading = false;
                        }
                    },
                    CancelAction = () => { }
                }
            );
        }   

        private bool CanRemoveTeams() => SelectedTeams.Count > 0;

        private void ApplyFilter()
        {
            TeamsView.Filter = o =>
            {
                if (o is not Team team)
                {
                    return false;
                }

                if (IsHideArchivedTeamsEnabled && team.IsArchived.HasValue && team.IsArchived.Value)
                {
                    return false;
                }

                return true;
            };
        }
    }
}
