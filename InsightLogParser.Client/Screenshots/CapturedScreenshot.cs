using InsightLogParser.Common.World;

namespace InsightLogParser.Client.Screenshots;

internal class CapturedScreenshot()
{
    public int PuzzleId { get; init; }
    public bool IsSolved { get; init; }
    public string ScreenshotPath { get; init; } = null!;
    public PuzzleType PuzzleType { get; init; }
}