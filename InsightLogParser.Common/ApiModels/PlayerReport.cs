using System.Linq;
using InsightLogParser.Common.World;

namespace InsightLogParser.Common.ApiModels;

public class PlayerReport
{
    /// <summary>
    /// The id of the puzzle
    /// </summary>
    public int PuzzleId { get; set; }

    /// <summary>
    /// The timestamp (from the log) when the puzzle was solved
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// The zone in which this puzzle was seen
    /// </summary>
    public PuzzleZone Zone { get; set; }

    /// <summary>
    /// The type of the puzzle
    /// </summary>
    public PuzzleType Type { get; set; }

    /// <summary>
    /// The server where this puzzle was seen
    /// </summary>
    public string? ServerAddress { get; set; }

    /// <summary>
    /// Indicates that the session started after the last cycle time of the puzzle
    /// </summary>
    /// <remarks>As some puzzles have a tendency to not de-spawn when cycled it is desired to mark presence as a "maybe"</remarks>
    public bool IsFresh { get; set; }
}