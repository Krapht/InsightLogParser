using System.Text.Json.Serialization;

namespace InsightLogParser.Client.Parsing;

public class ParsedLogEvent
{
    [JsonPropertyName("puzzle_id")]
    public int PuzzleId { get; set; }

    [JsonPropertyName("puzzle_name")]
    public string? PuzzleName { get; set; }

    [JsonPropertyName("puzzle_area")]
    public string? PuzzleArea { get; set; }

    [JsonPropertyName("game_mode")]
    public string? GameMode { get; set; }

    [JsonPropertyName("difficulty")]
    public short Difficulty { get; set; }

    [JsonPropertyName("event_type")]
    public string? EventType { get; set; }
}