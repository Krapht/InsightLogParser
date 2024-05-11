using InsightLogParser.Common.World;

namespace InsightLogParser.Common.PuzzleParser;

public class InsightPuzzle
{
    /// <summary>
    /// The id of the puzzle
    /// </summary>
    public int KrakenId { get; set; }

    /// <summary>
    /// True if the puzzle is part of the "world sandbox"
    /// </summary>
    public bool IsWorldPuzzle { get; set; }

    /// <summary>
    /// The type of puzzle
    /// </summary>
    public PuzzleType Type { get; set; }

    /// <summary>
    /// The Zone of the puzzle, if known
    /// </summary>
    public PuzzleZone Zone { get; set; }

    /// <summary>
    /// Ids of puzzles flagged as "incompatible", purpose unknown
    /// </summary>
    public IReadOnlyList<int> IncompatibleIds { get; set; } = [];
}