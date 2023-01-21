using Newtonsoft.Json;

namespace SchildTeamsManager.Settings
{
    public class JsonSettings : ISettings
    {
        [JsonProperty("schild")]
        public ISchildSettings SchILD { get; } = new JsonSchildSettings();

        [JsonProperty("graph")]
        public IGraphSettings Graph { get; } = new JsonGraphSettings();
    }
}
