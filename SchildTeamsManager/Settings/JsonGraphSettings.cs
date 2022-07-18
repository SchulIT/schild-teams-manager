using Newtonsoft.Json;

namespace SchildTeamsManager.Settings
{
    public class JsonGraphSettings : IGraphSettings
    {
        [JsonProperty("tenant_id")]
        public string TenantId { get; set; }

        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }
    }
}
