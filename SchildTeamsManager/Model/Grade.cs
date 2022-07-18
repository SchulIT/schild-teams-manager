using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace SchildTeamsManager.Model
{
    public class Grade : ObservableObject
    {
        public string Name { get; set; }

        public Teacher Teacher { get; set; }

        public Teacher AdditionalTeacher { get; set; }

        private Team associatedTeam;

        public Team AssociatedTeam
        {
            get => associatedTeam;
            set => SetProperty(ref associatedTeam, value);
        }
    }
}
