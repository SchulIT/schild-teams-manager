using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;

namespace SchildTeamsManager.Model
{
    public class Grade : ObservableObject
    {
        public string Name { get; set; }

        public Teacher Teacher { get; set; }

        public Teacher AdditionalTeacher { get; set; }

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
