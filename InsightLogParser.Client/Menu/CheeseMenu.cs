using System.Linq;

namespace InsightLogParser.Client.Menu;

internal class CheeseMenu : IMenu
{
    private readonly Spider _spider;
    private readonly MessageWriter _writer;

    public CheeseMenu(Spider spider, MessageWriter writer)
    {
        _spider = spider;
        _writer = writer;
    }

    public IEnumerable<(char? key, string text)> MenuOptions
    {
        get
        {
            yield return (null, "Cheesing. Press [h] to display this menu again and backspace to back");
            if (_spider.IsOnline())
            {
                yield return ('s', "Open solution to last opened puzzle, if available");
                yield return ('m', "Find mate to matchbox closest to last teleport");
                yield return ('t', "Target puzzle by id");
                yield return ('f', "List 15 closest (in 2d) puzzles to last teleport");
                yield return ('F', "List 15 closest (in 3d) puzzles to last teleport");

            }
        }
    }

    public async Task<MenuResult> HandleOptionAsync(char keyChar)
    {
        switch (keyChar)
        {
            case 's':
                _spider.OpenSolutionScreenshot();
                return MenuResult.Ok;
            case 'm':
                _spider.TargetOtherMatchbox();
                return MenuResult.Ok;
            case 't':
                Console.Write("Puzzle Id: ");
                var consoleString = Console.ReadLine();
                if (!int.TryParse(consoleString, out var puzzleId))
                {
                    _writer.WriteError("Could not understand puzzle id");
                    return MenuResult.Ok;
                }
                _spider.TargetPuzzle(puzzleId);
                return MenuResult.Ok;

            case 'f':
                await _spider.ListClosestAsync(15, true);
                return MenuResult.Ok;

            case 'F':
                await _spider.ListClosestAsync(15, false);
                return MenuResult.Ok;
            default:
                return MenuResult.NotValidOption;
        }
    }
}