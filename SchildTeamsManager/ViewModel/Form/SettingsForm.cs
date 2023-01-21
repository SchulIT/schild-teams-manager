using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SchildTeamsManager.ViewModel.Form
{
    public sealed partial class SettingsForm : ObservableValidator
    {
        [Required]
        [ObservableProperty]
        private string connectionString;

        [ObservableProperty]
        private bool onlyVisible;

        [Required]
        [ObservableProperty]
        private string tenantId;

        [Required]
        [ObservableProperty]
        private string clientId;

        [Required]
        [ObservableProperty]
        private string clientSecret;
    }
}
