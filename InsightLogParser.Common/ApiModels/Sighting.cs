using System.Text.Json.Serialization;

namespace InsightLogParser.Common.ApiModels;

public class Sighting
{
    [JsonPropertyName("puzzleId")]
    public int PuzzleId { get; set; }

    [JsonPropertyName("servers")]
    public List<string> ServerAddresses { get; set; } = new();

    [JsonPropertyName("isFresh")]
    public bool IsFresh { get; set; }
}