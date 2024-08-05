
namespace InsightLogParser.Client.Parsing;

public enum LogEventType
{
    SessionRestartHandshake,
    SessionEnd,
    PuzzleEvent,
    Teleport,
    JoinedServer,
    ConnectingToServer,
}