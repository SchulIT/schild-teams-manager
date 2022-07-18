using Newtonsoft.Json;

namespace SchildTeamsManager.Settings
{
    public class JsonSettings : ISettings
    {
        [JsonProperty("schild")]
        public ISchildSettings SchILD { get; } = new JsonSchildSettings();

        [JsonProperty("teams")]
        public ITeamsSettings Teams { get; } = new JsonTeamsSettings();

        [JsonProperty("graph")]
        public IGraphSettings Graph { get; } = new JsonGraphSettings();
    }
}
