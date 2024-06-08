using System.Text.Json.Serialization;

namespace InsightLogParser.Common.ApiModels;

public class PuzzleStatus
{
    [JsonPropertyName("puzzleId")]
    public int PuzzleId { get; set; }

    [JsonPropertyName("isSolved")]
    public bool IsSolved { get; set; }

    [JsonPropertyName("seenInCurrentCycle")]
    public bool SeenInCurrentCycle { get; set; }

    [JsonPropertyName("screenshotRequested")]
    public bool ScreenshotRequested { get; set; }
}