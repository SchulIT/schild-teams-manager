using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using SchildTeamsManager.Extension;
using SchildTeamsManager.Model;
using SchildTeamsManager.Service.MicrosoftGraph;
using SchildTeamsManager.Service.Teams;
using SchildTeamsManager.Settings;
using SchildTeamsManager.UI.Dialog;
using SchulIT.SchildExport;
using SchulIT.SchildExport.Linq;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SchildTeamsManager.ViewModel
{
    public class GradesViewModel : ObservableRecipient
    {
        #region Properties

        private bool isBusy;

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                SetProperty(ref isBusy, value);
                LoadGradesCommand?.NotifyCanExecuteChanged();
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

        private ObservableCollection<Grade> Grades { get; } = new ObservableCollection<Grade>();

        public ICollectionView GradesView { get; }

        #endregion

        #region Commands

        public AsyncRelayCommand LoadGradesCommand { get; }

        #endregion

        #region Services

        private readonly IMicrosoftGraph graph;
        private readonly IExporter schildExporter;
        private readonly ISettingsManager settingsManager;
        private readonly IDialogHelper dialogHelper;
        private readonly ITeamDataResolver aliasResolver;

        #endregion

        public GradesViewModel(IMicrosoftGraph graph, IExporter schildExporter, ISettingsManager settingsManager, IDialogHelper dialogHelper, ITeamDataResolver aliasResolver)
        {
            this.graph = graph;
            this.schildExporter = schildExporter;
            this.settingsManager = settingsManager;
            this.dialogHelper = dialogHelper;
            this.aliasResolver = aliasResolver;

            GradesView = CollectionViewSource.GetDefaultView(Grades);

            LoadGradesCommand = new AsyncRelayCommand(LoadGradesAsync, CanLoadGrades);
        }

        private bool CanLoadGrades() => !IsBusy;

        private async Task LoadGradesAsync()
        {
            if(settingsManager.Settings == null)
            {
                dialogHelper.Show(new Dialog { Title = "Fehler", Header = "Einstellungen nicht geladen", Content = "Die Anwendungseinstellungen wurden nicht ordnungsgemäß geladen. Bitte das Programm neu starten und erneut probieren." });
                return;
            }

            if(string.IsNullOrEmpty(settingsManager.Settings.SchILD.ConnectionString))
            {
                dialogHelper.Show(new Dialog { Title = "Fehler", Header = "SchILD-Einstellungen unvollständig", Content = "Bitte die Verbindungszeichenfolge für SchILD in den Einstellungen konfigurieren." });
                return;
            }

            schildExporter.Configure(settingsManager.Settings.SchILD.ConnectionString, false);

            if(IsBusy)
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

                var grades = await schildExporter.GetGradesAsync(SchoolYear, Section);
                var teachers = await schildExporter.GetTeachersAsync();


                if (settingsManager.Settings.SchILD.OnlyVisibleEntities)
                {
                    grades = grades.WhereIsVisible().ToList();
                    teachers = teachers.WhereIsVisible().ToList();
                }

                Grades.Clear();

                var teams = await graph.GetGradeTeamsAsync(SchoolYear);

                foreach(var schildGrade in grades)
                {
                    var teacher = teachers.FirstOrDefault(x => schildGrade.Teacher != null && x.Acronym == schildGrade.Teacher.Acronym);
                    var additionalTeacher = teachers.FirstOrDefault(x => schildGrade.SubstituteTeacher != null && x.Acronym == schildGrade.SubstituteTeacher.Acronym);

                    var grade = new Grade
                    {
                        Name = schildGrade.Name,
                        Teacher = ConvertTeacher(teacher),
                        AdditionalTeacher = ConvertTeacher(additionalTeacher),
                    };

                    grade.AssociatedTeam = teams.FirstOrDefault(x => x.EmailAddress != null && x.EmailAddress.StartsWith(aliasResolver.ResolveAlias(grade, SchoolYear)));

                    Grades.Add(grade);
                }
            }
            catch (Exception e)
            {
                dialogHelper.Show(new ErrorDialog { Title = "Fehler", Header = "Fehler bei der Abfrage", Content = "Es scheint ein Problem mit der Verbindung zur SchILD-Datenbank zu geben.", Exception = e });
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static Teacher ConvertTeacher(SchulIT.SchildExport.Models.Teacher teacher)
        {
            if(teacher == null)
            {
                return null;
            }

            return new Teacher
            {
                Firstname = teacher.Firstname,
                Lastname = teacher.Lastname,
                EmailAddress = teacher.Email
            };
        }
    }
}
