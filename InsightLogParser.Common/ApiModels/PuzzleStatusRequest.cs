using System.Text.Json.Serialization;

namespace InsightLogParser.Common.ApiModels;

public class PuzzleStatusRequest
{
    [JsonPropertyName("puzzleIds")]
    public int[] PuzzleIds { get; set; } = [];
}