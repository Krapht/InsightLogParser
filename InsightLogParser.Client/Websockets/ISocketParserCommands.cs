using InsightLogParser.Common.ApiModels;

namespace InsightLogParser.Client.Websockets;

/// <summary>
/// Encapsulates all web socket commands available in the parser
/// </summary>
public interface ISocketParserCommands
{
    /// <summary>
    /// Retrieves screenshots for a puzzle
    /// </summary>
    /// <param name="puzzleId">The id of the puzzle</param>
    /// <returns>Any found screenshots or null if screenshots can't be retrieved</returns>
    Task<PuzzleScreenshotsDetails[]?> GetPuzzleScreenshotsAsync(int puzzleId);

    /// <summary>
    /// Advances the route to the next node
    /// </summary>
    /// <returns>True if the node could be advanced, otherwise false</returns>
    bool RouteNext();

    /// <summary>
    /// Moves the route to the previous node
    /// </summary>
    /// <returns>True if successful, otherwise false</returns>
    bool RoutePrev();
}
