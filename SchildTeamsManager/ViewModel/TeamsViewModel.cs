using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchildTeamsManager.Model;
using SchildTeamsManager.Service.MicrosoftGraph;
using SchildTeamsManager.UI;
using SchildTeamsManager.UI.Dialog;
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

        private bool isOnlyShowSchoolYearEnabled;

        public bool IsOnlyShowSchoolYearEnabled
        {
            get { return isOnlyShowSchoolYearEnabled; }
            set
            {
                SetProperty(ref isOnlyShowSchoolYearEnabled, value);
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

        public AsyncRelayCommand LoadTeamsCommand { get; private set; }

        public AsyncRelayCommand ArchiveTeamsCommand { get; private set; }

        public AsyncRelayCommand UnarchiveTeamsCommand { get; private set; }

        #endregion

        #region Services

        private readonly IMicrosoftGraph graph;
        private readonly IDialogHelper dialogHelper;

        #endregion

        public TeamsViewModel(IMicrosoftGraph graph, IDialogHelper dialogHelper)
        {
            this.graph = graph;
            this.dialogHelper = dialogHelper;
            LoadTeamsCommand = new AsyncRelayCommand(LoadTeams);
            ArchiveTeamsCommand = new AsyncRelayCommand(ArchiveTeams, CanArchiveTeams);
            UnarchiveTeamsCommand = new AsyncRelayCommand(UnarchiveTeams, CanUnarchiveTeams);

            TeamsView = CollectionViewSource.GetDefaultView(Teams);
            SelectedTeams.CollectionChanged += delegate
            {
                ArchiveTeamsCommand?.NotifyCanExecuteChanged();
                UnarchiveTeamsCommand?.NotifyCanExecuteChanged();
            };
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
                var teams = await graph.GetTeamsAsync();

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
                for(int idx = 0; idx < teams.Count / 20; idx++)
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

                if(IsOnlyShowSchoolYearEnabled&& SchoolYear > 0 && !team.EmailAddress.EndsWith(SchoolYear + "" + (SchoolYear % 100 + 1)))
                {
                    return false;
                }

                return true;
            };
        }
    }
}
