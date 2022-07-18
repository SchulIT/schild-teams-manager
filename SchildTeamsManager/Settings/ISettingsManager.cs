using System.Threading.Tasks;

namespace SchildTeamsManager.Settings
{
    public interface ISettingsManager
    {
        ISettings Settings { get; }

        Task LoadSettingsAsync();

        Task SaveSettingsAsync();
    }
}
