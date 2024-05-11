using System.Linq;
using System.Text.Json.Serialization;
using InsightLogParser.Common.World;

namespace InsightLogParser.Common.ApiModels;

public class ZoneStatistics
{
    [JsonPropertyName("puzzleType")]
    public PuzzleType PuzzleType { get; set; }

    [JsonPropertyName("expectedCount")]
    public int? ExpectedCount { get; set; }

    [JsonPropertyName("sightings")]
    public List<Sighting> Sightings { get; set; } = new();
}
