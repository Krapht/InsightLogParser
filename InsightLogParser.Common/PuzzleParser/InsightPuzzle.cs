using System.Diagnostics;
using InsightLogParser.Common.World;

namespace InsightLogParser.Common.PuzzleParser;

[DebuggerDisplay("{KrakenId}-{Zone}-{Type}")]
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

    /// <summary>
    /// The primary coordinate of a puzzle
    /// </summary>
    public Coordinate? PrimaryCoordinate { get; set; }

    /// <summary>
    /// The secondary coordinate of a puzzle, currently only used by matchboxes
    /// </summary>
    public Coordinate? SecondaryCoordinate { get; set; }
}