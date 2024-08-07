using InsightLogParser.Common.Screenshots;
using System.Linq;
using System.Text.Json.Serialization;

namespace InsightLogParser.Common.ApiModels
{
    public class PuzzleScreenshotsDetails
    {
        [JsonPropertyName("category")]
        public required ScreenshotCategory Category { get; set; }

        [JsonPropertyName("thumbUrl")]
        public required string ThumbUrl { get; set; }

        [JsonPropertyName("imageUrl")]
        public required string ImageUrl { get; set; }

        [JsonPropertyName("isPrimaryCategory")]
        public required bool IsPrimaryCategory { get; set; }

        [JsonPropertyName("uploader")]
        public string? Uploader { get; set; }
    }
}
