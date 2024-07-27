
using System.Text.Json.Serialization;
using InsightLogParser.Common.Screenshots;

namespace InsightLogParser.Common.ApiModels
{
    public class SeenResponse
    {
        [JsonPropertyName("firstSighting")]
        public required bool FirstSighting { get; set; }

        [JsonPropertyName("screenshots")]
        public ScreenshotCategory[] Screenshots { get; set; }
    }
}
