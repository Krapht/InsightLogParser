using System.Text.Json.Serialization;

namespace InsightLogParser.Common.ApiModels
{
    public class ScreenshotResponse
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = null!;
    }
}
