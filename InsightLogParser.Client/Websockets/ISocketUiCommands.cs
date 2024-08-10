using System.Net.WebSockets;
using InsightLogParser.Common.PuzzleParser;
using InsightLogParser.Common.World;

namespace InsightLogParser.Client.Websockets;

public interface ISocketUiCommands
{
    /// <summary>
    /// Sets the current target
    /// </summary>
    /// <param name="target">The coordinate of the target</param>
    /// <param name="puzzle">The target puzzle</param>
    /// <param name="routeNumber">The number of the current route node or 0 if not on route</param>
    /// <param name="routeLength">The length of the route or 0 if not on route</param>
    /// <param name="webSocket">The websocket to send to, or everyone if not specified</param>
    void SetTarget(Coordinate target, InsightPuzzle puzzle, int routeNumber, int routeLength, WebSocket? webSocket = null);

    /// <summary>
    /// Updates the player position
    /// </summary>
    /// <param name="destination">The coordinate of the last known player position</param>
    /// <param name="webSocket">The websocket to send to, or everyone if not specified</param>
    void MovePlayer(Coordinate destination, WebSocket? webSocket = null);

    /// <summary>
    /// Updates the connection state
    /// </summary>
    /// <param name="isConnected">Whether the user is connected to the game</param>
    /// <param name="ipAddress">The IP address of the server the user is connected to</param>
    /// <param name="webSocket">The websocket to send to, or everyone if not specified</param>
    void SetConnection(bool isConnected, string ipAddress, WebSocket? webSocket = null ) { }
}

internal class DummySocketUiCommands : ISocketUiCommands
{
    //No-op
    public void SetTarget(Coordinate target, InsightPuzzle puzzle, int routeNumber, int routeLength, WebSocket? webSocket) { }

    //No-op
    public void MovePlayer(Coordinate destination, WebSocket? webSocket) { }

    //No-op
    public void SetConnection(bool isConnected, string ipAddress, WebSocket? webSocket) { }
}