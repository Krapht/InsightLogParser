using System.Text.Json.Serialization;
using InsightLogParser.Common.Screenshots;

namespace InsightLogParser.Common.ApiModels
{
    public class Screenshot
    {
        [JsonPropertyName("puzzleId")]
        public int PuzzleId { get; set; }

        [JsonPropertyName("category")]
        public ScreenshotCategory Category { get; set; }

        [JsonPropertyName("screenshotBytes")]
        public byte[] ScreenshotBytes { get; set; }

        [JsonPropertyName("thumbBytes")]
        public byte[]? ThumbBytes { get; set; }

    }
}
