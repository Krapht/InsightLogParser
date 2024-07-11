using InsightLogParser.Common.PuzzleParser;

namespace InsightLogParser.Client.Routing;

internal class RouteNode
{
    public required InsightPuzzle Puzzle { get; init; }
    public required bool? Stale { get; init; }
    public required string[]? Servers { get; init; }
}