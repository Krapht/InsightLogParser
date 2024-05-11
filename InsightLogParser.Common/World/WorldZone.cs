
namespace InsightLogParser.Common.World;

public class WorldZone
{
    public string Name { get; set; } = null!;
    public PuzzleZone Zone { get; set; }
    public Dictionary<PuzzleType, PuzzleInformation> Puzzles { get; set; } = new();
}