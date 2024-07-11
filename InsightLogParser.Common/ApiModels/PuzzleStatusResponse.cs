using System.Text.Json.Serialization;

namespace InsightLogParser.Common.ApiModels;

public class PuzzleStatusResponse
{
    [JsonPropertyName("puzzleStatus")]
    public Dictionary<int, PuzzleStatus> PuzzleStatus { get; set; } = new();
}