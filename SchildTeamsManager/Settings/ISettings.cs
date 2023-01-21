namespace SchildTeamsManager.Settings
{
    public interface ISettings
    {
        ISchildSettings SchILD { get; }

        IGraphSettings Graph { get; }
    }
}
