namespace SchildTeamsManager.Settings
{
    public interface IGraphSettings
    {
        string TenantId { get; set; }

        string ClientId { get; set; }

        string ClientSecret { get; set; }
    }
}
