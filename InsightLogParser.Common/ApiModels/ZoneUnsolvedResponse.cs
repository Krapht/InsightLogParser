using System.Text.Json.Serialization;

namespace InsightLogParser.Common.ApiModels;

public class ZoneUnsolvedResponse
{
    [JsonPropertyName("unsolved")]
    public List<ZoneUnsolved> Unsolved { get; set; }
}

public class ZoneUnsolved
{
    [JsonPropertyName("puzzleId")]
    public int PuzzleId { get; set; }

    [JsonPropertyName("isStale")]
    public bool IsStale { get; set; }

    [JsonPropertyName("servers")]
    public string[] Servers { get; set; }
}