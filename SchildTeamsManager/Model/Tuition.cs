using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Linq;

namespace SchildTeamsManager.Model
{
    public class Tuition : ObservableObject
    {
        public int? SchildId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public Grade[] Grades { get; set; } = Array.Empty<Grade>();

        public string GradesAsString { get; set; } = string.Empty;

        public Teacher[] Teachers { get; set; } = Array.Empty<Teacher>();

        public string TeachersAsString
        {
            get
            {
                return string.Join(", ", Teachers.AsEnumerable());
            }
        }

        public Student[] Students { get; set; } = Array.Empty<Student>();

        private Team? associatedTeam;

        public Team? AssociatedTeam
        {
            get => associatedTeam;
            set => SetProperty(ref associatedTeam, value);
        }

        private bool isBusy;

        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

    }
}
