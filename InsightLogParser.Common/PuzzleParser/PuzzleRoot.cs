using System.Text.Json.Serialization;

namespace InsightLogParser.Common.PuzzleParser;

internal class PuzzleRoot
{
    [JsonPropertyName("puzzles")]
    public Puzzle[]? Puzzles { get; set; }
}