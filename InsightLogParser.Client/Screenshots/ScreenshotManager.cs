using InsightLogParser.Common.PuzzleParser;
using InsightLogParser.Common.Screenshots;
using InsightLogParser.Common.World;
using System.Linq;

namespace InsightLogParser.Client.Screenshots;

internal class ScreenshotManager
{
    private readonly GamePuzzleHandler _gamePuzzleHandler;
    private readonly MessageWriter _messageWriter;
    private readonly UserComputer _userComputer;
    private readonly Configuration _configuration;
    private string? _lastScreenshotPath = null;
    private int? _lastSolvedPuzzle = null;
    private Action<CapturedScreenshot>? _callback = null;
    private bool _isSolved;

    public ScreenshotManager(GamePuzzleHandler gamePuzzleHandler
        , MessageWriter messageWriter
        , UserComputer userComputer
        , Configuration configuration
    )
    {
        _gamePuzzleHandler = gamePuzzleHandler;
        _messageWriter = messageWriter;
        _userComputer = userComputer;
        _configuration = configuration;
    }

    public void SetLastPuzzle(int puzzleId, bool solved)
    {
        _lastSolvedPuzzle = puzzleId;
        _isSolved = solved;
        RefreshScreenshotMenu();
    }

    private void RefreshScreenshotMenu()
    {
        if (_lastScreenshotPath != null && _lastSolvedPuzzle != null)
        {
            _gamePuzzleHandler.PuzzleDatabase.TryGetValue(_lastSolvedPuzzle.Value, out var puzzle);
            if (puzzle == null) return;

            var captured = new CapturedScreenshot
            {
                PuzzleId = _lastSolvedPuzzle.Value,
                IsSolved = _isSolved,
                ScreenshotPath = _lastScreenshotPath,
                PuzzleType = puzzle.Type,
            };
            _callback?.Invoke(captured);
        }
    }

    public void FlagScreenshotAsHandled()
    {
        _lastScreenshotPath = null;
    }

    public void NewScreenshot(string screenshotPath)
    {
        _messageWriter.WriteDebug($"Detected new screenshot '{screenshotPath}'");
        _lastScreenshotPath = screenshotPath;
        RefreshScreenshotMenu();
    }

    public void SetCallback(Action<CapturedScreenshot> callback)
    {
        _callback = callback;
    }

    public void DeleteScreenshot(string path)
    {
        _userComputer.DeleteScreenshot(path);
        FlagScreenshotAsHandled();
    }

    public static string  GetCategoryName(ScreenshotCategory category)
    {
        switch (category)
        {
            case ScreenshotCategory.Other:
                return "Other";
            case ScreenshotCategory.Initial:
                return "Initial";
            case ScreenshotCategory.Solved:
                return "Solved";
            case ScreenshotCategory.Location:
                return "Location";
            case ScreenshotCategory.Scenic:
                return "Scenic";
            default:
                return $"TODO: {category}";
        }
    }

    public IEnumerable<(ScreenshotCategory Category, bool IsMissing)> GetScreenshotStatus(PuzzleType type, bool isSolved, ScreenshotCategory[] currentScreenshots)
    {
        var categories = GetScreenshotCategories(type, isSolved);
        foreach (var category in categories)
        {
            if (category.Category == ScreenshotCategory.Scenic && !_configuration.EnableScenicScreenshots) continue;

            if (currentScreenshots.Contains(category.Category))
            {
                yield return (category.Category, false);
            }
            else if (category.IsRequested)
            {
                yield return (category.Category, true);
            }
        }
    }

    public static IEnumerable<(ScreenshotCategory Category, string Description, bool IsDefault, bool IsRequested)> GetScreenshotCategories(PuzzleType type, bool isSolved)
    {
        const string otherDescription = "Any screenshots that doesn't fit another category";
        switch (type)
        {
            //Location puzzles
            case PuzzleType.WanderingEcho:
            case PuzzleType.MatchBox:
            case PuzzleType.HiddenArchway:
            case PuzzleType.HiddenPentad:
            case PuzzleType.HiddenRing:
            case PuzzleType.HiddenCube:
            case PuzzleType.ShyAura:
            case PuzzleType.GlideRings:
                yield return (ScreenshotCategory.Location, "Where in the world you can find the puzzle", true, true);
                yield return (ScreenshotCategory.Other, otherDescription, false, false);
                break;

            case PuzzleType.FlowOrbs:
                yield return (ScreenshotCategory.Location, "Where in the world you can find the puzzle", true, true);
                yield return (ScreenshotCategory.Scenic, "Taken before first solve (no time recorded), florb selected, clear view of the puzzle. Manual moderation", false, true);
                yield return (ScreenshotCategory.Other, otherDescription, false, false);
                break;

            case PuzzleType.SightSeer: //Hard to categorize
            case PuzzleType.LightMotif:
                yield return (ScreenshotCategory.Solved, "The view you have when you solve the puzzle", true, true);
                yield return (ScreenshotCategory.Location, "Context to where the puzzle can be found", false, false);
                yield return (ScreenshotCategory.Other, otherDescription, false, false);
                break;

            //Initial state puzzles that must be screenshotted before solving
            case PuzzleType.RollingBlock:
                yield return (ScreenshotCategory.Initial, "The initial state of the puzzle", true, true);
                yield return (ScreenshotCategory.Other, otherDescription, false, false);
                break;

            //Initial state wall puzzles that has no real point in screenshotting the solution
            case PuzzleType.MatchThree:
            case PuzzleType.PhasicDial:
            case PuzzleType.MorphicFractal:
            case PuzzleType.ShiftingMosaic:
                yield return (ScreenshotCategory.Initial, "The initial state of the puzzle", isSolved == false, true);
                yield return (ScreenshotCategory.Other, otherDescription, isSolved == true, false);
                break;

            //Initial and solved state puzzle boxes
            case PuzzleType.LogicGrid:
            case PuzzleType.PatternGrid:
                yield return (ScreenshotCategory.Initial, "The initial state of the puzzle", isSolved == false, true);
                yield return (ScreenshotCategory.Solved, "The solved state of the puzzle", isSolved == true, true);
                yield return (ScreenshotCategory.Other, otherDescription, false, false);
                break;
            case PuzzleType.MusicGrid:
            case PuzzleType.MemoryGrid:
                yield return (ScreenshotCategory.Initial, "The initial state of the puzzle", isSolved == false, false);
                yield return (ScreenshotCategory.Solved, "The solved state of the puzzle", isSolved == true, true);
                yield return (ScreenshotCategory.Other, otherDescription, false, false);
                break;

            //I guess you could want to take screenshots of both the initial and solved state of sentinels. I'll support it, but won't request it
            case PuzzleType.SentinelStones:
                yield return (ScreenshotCategory.Initial, "The initial state of the puzzle", false, false);
                yield return (ScreenshotCategory.Solved, "The solved state of the puzzle", false, false);
                yield return (ScreenshotCategory.Other, otherDescription, false, false);
                break;

            //Put these here for now
            case PuzzleType.CrystalLabyrinth:
            case PuzzleType.ArmillaryRings:
            case PuzzleType.Skydrop:
                yield return (ScreenshotCategory.Other, otherDescription, false, false);
                break;

            case PuzzleType.Unknown:
            default:
                break;
        }
    }
}