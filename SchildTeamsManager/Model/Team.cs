using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace SchildTeamsManager.Model
{
    public class Team : ObservableObject
    {
        public string GroupId { get; set; } = string.Empty;

        private bool? isArchived = null;

        public bool? IsArchived
        {
            get => isArchived;
            set => SetProperty(ref isArchived, value);
        }

        public string DisplayName { get; set; } = string.Empty;

        public string EmailAddress { get; set; } = string.Empty;

        public bool AreMembersFetched { get; set; } = false;

        public ObservableCollection<User> Owners { get; set; } = new ObservableCollection<User>();

        public ObservableCollection<User> Members { get; set; } = new ObservableCollection<User>();
    }
}
