using Newtonsoft.Json;

namespace SchildTeamsManager.Settings
{
    public class JsonTeamsSettings : ITeamsSettings
    {
        [JsonProperty("tuition_pattern")]
        public string TuitionNamePattern { get; set; } = "%klasse% %fach% (%schuljahr%)";

        [JsonProperty("grade_pattern")]
        public string GradeNamePattern { get; set; } = "%klasse% Ordinariat (%schuljahr%)";
    }
}
