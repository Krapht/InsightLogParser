using InsightLogParser.Common.World;

namespace InsightLogParser.Common.ParsedDbModels;

public class ParsedDb
{
    public int Version { get; set; }
    public Dictionary<PuzzleZone, ZoneNode> Zones { get; set; } = new();
}