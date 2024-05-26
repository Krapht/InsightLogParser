using System.Text.Json.Serialization;

namespace InsightLogParser.Common.ApiModels;

public class ZoneStatisticsResponse
{
    [JsonPropertyName("puzzles")]
    public List<ZoneStatistics> Puzzles { get; set; } = new();
}