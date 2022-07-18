using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using NaturalSort.Extension;
using SchildTeamsManager.Model;
using SchildTeamsManager.Service.MicrosoftGraph;
using SchildTeamsManager.Service.Schild;
using SchildTeamsManager.Service.Teams;
using SchildTeamsManager.Settings;
using SchildTeamsManager.UI.Dialog;
using SchulIT.SchildExport;
using SchulIT.SchildExport.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SchildTeamsManager.ViewModel
{
    public class TuitionsViewModel : ObservableRecipient
    {
        #region Properties

        private bool isBusy;

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                SetProperty(ref isBusy, value);
                LoadTuitionsCommand?.NotifyCanExecuteChanged();
                CreateTeamsCommand?.NotifyCanExecuteChanged();
            }
        }

        private short schoolYear;

        public short SchoolYear
        {
            get { return schoolYear; }
            set
            {
                SetProperty(ref schoolYear, value);
            }
        }

        private short section;

        public short Section
        {
            get { return section; }
            set
            {
                SetProperty(ref section, value);
            }
        }

        private ObservableCollection<Tuition> Tuitions { get; } = new ObservableCollection<Tuition>();

        public ICollectionView TuitionsView { get; }

        public ObservableCollection<Tuition> SelectedTuitions { get; } = new ObservableCollection<Tuition>();

        #endregion

        #region Commands

        public AsyncRelayCommand LoadTuitionsCommand { get; }

        public AsyncRelayCommand CreateTeamsCommand { get; }

        #endregion

        #region Services

        private readonly IMicrosoftGraph graph;
        private readonly IExporter schildExporter;
        private readonly ISettingsManager settingsManager;
        private readonly IDialogHelper dialogHelper;
        private readonly INameResolver nameResolver;
        private readonly ITeamDataResolver aliasResolver;

        #endregion

        public TuitionsViewModel(IMicrosoftGraph graph, IExporter schildExporter, ISettingsManager settingsManager, IDialogHelper dialogHelper, INameResolver nameResolver, ITeamDataResolver aliasResolver)
        {
            this.graph = graph;
            this.schildExporter = schildExporter;
            this.settingsManager = settingsManager;
            this.dialogHelper = dialogHelper;
            this.nameResolver = nameResolver;
            this.aliasResolver = aliasResolver;

            TuitionsView = CollectionViewSource.GetDefaultView(Tuitions);
            TuitionsView.GroupDescriptions.Add(new PropertyGroupDescription("GradesAsString"));
            TuitionsView.SortDescriptions.Add(new SortDescription("GradesAsString", ListSortDirection.Ascending));

            LoadTuitionsCommand = new AsyncRelayCommand(LoadTuitionsAsync, CanLoadTuitions);
            CreateTeamsCommand = new AsyncRelayCommand(CreateTeams, CanCreateTeams);

            SelectedTuitions.CollectionChanged += delegate
            {
                CreateTeamsCommand?.NotifyCanExecuteChanged();
            };
        }

        private async Task CreateTeams()
        {
            try
            {
                IsBusy = true;

                var tuitions = SelectedTuitions.ToList(); // Kopie erstellen

                var tasks = new List<Task<Team>>();

                foreach (var tuition in tuitions)
                {
                    tuition.IsBusy = true;
                    tasks.Add(graph.CreateTeamAsync(aliasResolver.ResolveDisplayName(tuition, SchoolYear), aliasResolver.ResolveAlias(tuition, SchoolYear), tuition.Teachers.Select(x => x.EmailAddress).ToList(), tuition.Students.Select(x => x.EmailAddress).ToList(), "eduClass"));
                }

                var teams = await Task.WhenAll(tasks);

            }
            catch (Exception ex)
            {
                dialogHelper.Show(new ErrorDialog { Title = "Fehler", Header = "Fehler beim Erstellen des Teams", Content = "Bitte die Details anschauen zwecks Fehlerursache.", Exception = ex });
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanCreateTeams() => !IsBusy && SelectedTuitions.Count > 0;

        private async Task LoadTuitionsAsync()
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

            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;

                if (schoolYear == default(short) || section == default(short))
                {
                    var schoolInfo = await schildExporter.GetSchoolInfoAsync();

                    if (schoolInfo.CurrentYear != null && schoolInfo.CurrentSection != null)
                    {
                        SchoolYear = schoolInfo.CurrentYear.Value;
                        Section = schoolInfo.CurrentSection.Value;
                    }
                }

                var students = await schildExporter.GetStudentsAsync(SchoolYear, Section);
                var tuitions = await schildExporter.GetTuitionsAsync(students, SchoolYear, Section);
                var teachers = await schildExporter.GetTeachersAsync();
                var studyGroups = await schildExporter.GetStudyGroupsAsync(students, SchoolYear, Section);

                if (settingsManager.Settings.SchILD.OnlyVisibleEntities)
                {
                    tuitions = tuitions.WhereStudyGroupIsVisible().ToList();
                    teachers = teachers.WhereIsVisible().ToList();
                }

                var teams = await graph.GetTeamsAsync(SchoolYear);

                Tuitions.Clear();

                foreach(var tuition in tuitions)
                {
                    var studyGroup = studyGroups.FirstOrDefault(x => x.Id == tuition.StudyGroupRef.Id && tuition.StudyGroupRef.Name == x.Name);

                    if(studyGroup == null)
                    {
                        continue;
                    }

                    var grades = studyGroup.Grades.Select(x => new Grade { Name = x.Name }).ToArray();

                    var tuitionTeachers = new List<Teacher>();

                    if(tuition.TeacherRef != null)
                    {
                        var schildTeacher = teachers.FirstOrDefault(x => x.Acronym == tuition.TeacherRef.Acronym);

                        if (schildTeacher == null)
                        {
                            continue;
                        }

                        tuitionTeachers.Add(new Teacher
                        {
                            Firstname = schildTeacher.Firstname,
                            Lastname = schildTeacher.Lastname,
                            EmailAddress = schildTeacher.Email
                        });
                    }

                    foreach(var teacher in tuition.AdditionalTeachersRef)
                    {
                        var schildTeacher = teachers.FirstOrDefault(x => x.Acronym == teacher.Acronym);

                        if(schildTeacher == null)
                        {
                            continue;
                        }

                        tuitionTeachers.Add(new Teacher
                        {
                            Firstname = schildTeacher.Firstname,
                            Lastname = schildTeacher.Lastname,
                            EmailAddress = schildTeacher.Email
                        });
                    }

                    var tuitionStudents = new List<Student>();

                    foreach(var membership in studyGroup.Memberships)
                    {
                        var student = students.FirstOrDefault(x => x.Id == membership.Student.Id);

                        if(student != null)
                        {
                            tuitionStudents.Add(new Student
                            {
                                Firstname = student.Firstname,
                                Lastname = student.Lastname,
                                EmailAddress = student.Email,
                                Grade = student.Grade.Name
                            });
                        }
                    }

                    var tuitionModel = new Tuition
                    {
                        SchildId = tuition.StudyGroupRef.Id,
                        Name = nameResolver.ResolveName(tuition),
                        Subject = tuition.SubjectRef.Abbreviation,
                        Grades = grades,
                        GradesAsString = string.Join(", ", grades.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase.WithNaturalSort()).Select(x => x.Name)),
                        Teachers = tuitionTeachers.ToArray(),
                        Students = tuitionStudents.ToArray()
                    };

                    var alias = aliasResolver.ResolveAlias(tuitionModel, SchoolYear);

                    tuitionModel.AssociatedTeam = teams.FirstOrDefault(x => x.EmailAddress == alias);

                    Tuitions.Add(tuitionModel);
                }
            }
            catch (Exception ex)
            {
                dialogHelper.Show(new ErrorDialog { Title = "Fehler", Header = "Fehler bei der Abfrage", Content = "Es scheint ein Problem mit der Verbindung zur SchILD-Datenbank zu geben.", Exception = ex });
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanLoadTuitions() => !IsBusy;
    }
}
