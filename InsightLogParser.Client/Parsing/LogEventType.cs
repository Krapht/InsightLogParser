
namespace InsightLogParser.Client.Parsing;

public enum LogEventType
{
    SessionStart,
    SessionRestartHandshake,
    SessionEnd,
    PuzzleEvent,
    Teleport,
    JoinedServer,
}