using System.Text.Json.Serialization;

namespace InsightLogParser.Common.PuzzleParser;

public class Puzzle
{
    [JsonPropertyName("pid")]
    public int Pid { get; set; }

    [JsonPropertyName("serialized")]
    public string? Serialized { get; set; }

    [JsonPropertyName("puzzleType")]
    public string? PuzzleType { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("pdata")]
    public string? PData { get; set; }

    [JsonPropertyName("zone")]
    public string? Zone { get; set; }

    [JsonPropertyName("difficulty")]
    public int? Difficulty { get; set; }
}