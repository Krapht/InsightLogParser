using System.Text.Json.Serialization;
using InsightLogParser.Common.World;

namespace InsightLogParser.Common.ParsedDbModels;

public class ZoneNode
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PuzzleZone Zone { get; set; }

    public Dictionary<PuzzleType, Dictionary<int, PuzzleNode>> SolvedEntries { get; set; } = new();
}