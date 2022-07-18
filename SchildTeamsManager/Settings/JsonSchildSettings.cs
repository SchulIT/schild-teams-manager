using Newtonsoft.Json;
using System;

namespace SchildTeamsManager.Settings
{
    public class JsonSchildSettings : ISchildSettings
    {
        [JsonProperty("only_visible")]
        public bool OnlyVisibleEntities { get; set; } = true;

        [JsonProperty("student_status")]
        public int[] StudentFilter { get; set; } = Array.Empty<int>();

        [JsonProperty("connection_string")]
        public string ConnectionString { get; set; } = string.Empty;
    }
}
