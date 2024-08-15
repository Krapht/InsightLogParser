
using System.Text.Json.Serialization;

namespace InsightLogParser.Common.ApiModels
{
    public class PlayerPresence
    {
        [JsonPropertyName("serverIp")]
        public string? ServerIp { get; set; }
    }
}
