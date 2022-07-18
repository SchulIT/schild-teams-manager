namespace SchildTeamsManager.Settings
{
    public interface ISettings
    {
        ISchildSettings SchILD { get; }

        ITeamsSettings Teams { get; }

        IGraphSettings Graph { get; }
    }
}
