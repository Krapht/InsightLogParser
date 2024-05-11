
namespace InsightLogParser.Client.Parsing;

public enum LogEventType
{
    SessionStart,
    PlayerIdentified,
    SessionRestartHandshake,
    SessionEnd,
    PuzzleEvent,
    Teleport,
    JoinedServer,
}