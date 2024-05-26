
using System.Text.Json.Serialization;
using InsightLogParser.Common.Screenshots;

namespace InsightLogParser.Common.ApiModels
{
    public class SeenResponse
    {
        [JsonPropertyName("screenshots")]
        public ScreenshotCategory[] Screenshots { get; set; }
    }
}
