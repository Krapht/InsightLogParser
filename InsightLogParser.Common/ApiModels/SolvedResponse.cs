using System.Text.Json.Serialization;

namespace InsightLogParser.Common.ApiModels;

public class SolvedResponse
{
    [JsonPropertyName("sightings")]
    public Sighting[]? Sightings { get; set; }

}

public class ZoneStatisticsResponse
{
    [JsonPropertyName("puzzles")]
    public List<ZoneStatistics> Puzzles { get; set; } = new();
}