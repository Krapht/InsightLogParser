using InsightLogParser.Common.PuzzleParser;
using InsightLogParser.Common.World;

namespace InsightLogParser.Client.Websockets;

public interface ISocketUiCommands
{
    /// <summary>
    /// Sets the current target
    /// </summary>
    /// <param name="coordinate">The coordinate of the target</param>
    /// <param name="puzzle">The target puzzle</param>
    /// <param name="routeNumber">The number of the current route node or 0 if not on route</param>
    /// <param name="routeLength">The length of the route or 0 if not on route</param>
    void SetTarget(Coordinate coordinate, InsightPuzzle puzzle, int routeNumber, int routeLength);

    /// <summary>
    /// Updates the player position
    /// </summary>
    /// <param name="coordinate">The coordinate of the last known player position</param>
    void MovePlayer(Coordinate coordinate);
}

internal class DummySocketUiCommands : ISocketUiCommands
{
    //No-op
    public void SetTarget(Coordinate coordinate, InsightPuzzle puzzle, int routeNumber, int routeLength) { }

    //No-op
    public void MovePlayer(Coordinate coordinate) { }
}