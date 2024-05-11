using InsightLogParser.Common.World;
using System.Text.Json.Serialization;

namespace InsightLogParser.Common.ApiModels;

/*
 * This is not used by the client
 */

public class SetExpectedRequest()
{
    [JsonPropertyName("values")]
    public List<ExpectedValue> Values { get; set; } = new();
}

public class ExpectedValue
{
    [JsonPropertyName("zone")]
    public PuzzleZone Zone { get; set; }

    [JsonPropertyName("type")]
    public PuzzleType Type { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set;}
}