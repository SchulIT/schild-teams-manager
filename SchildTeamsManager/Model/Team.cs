using CommunityToolkit.Mvvm.ComponentModel;

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
    }
}
