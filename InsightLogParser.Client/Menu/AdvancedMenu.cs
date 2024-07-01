using System.Linq;

namespace InsightLogParser.Client.Menu;

internal class AdvancedMenu : IMenu
{
    private readonly MenuHandler _menuHandler;
    private readonly UserComputer _computer;
    private readonly MessageWriter _writer;
    private readonly Spider _spider;

    public AdvancedMenu(MenuHandler menuHandler, UserComputer computer, MessageWriter writer, Spider spider)
    {
        _menuHandler = menuHandler;
        _computer = computer;
        _writer = writer;
        _spider = spider;
    }

    public IEnumerable<(char? key, string text)> MenuOptions
    {
        get
        {
            yield return (null, "Advanced or risky operations. Press [h] to display this menu again and backspace to back");
            if (_spider.IsOnline())
            {
                yield return ('w', "Log-in to cetus web ui");
            }
            yield return ('c', "Cleanup steam screenshot.vdf file (at your own risk)");
            yield return ('t', "Target puzzle by id");
            yield return ('f', "List 15 closest (in 2d) puzzles to last teleport");
            yield return ('F', "List 15 closest (in 3d) puzzles to last teleport");
        }
    }

    public async Task<MenuResult> HandleOptionAsync(char keyChar)
    {
        switch (keyChar)
        {
            case 'w':
                await _spider.OpenCetusWebAsync().ConfigureAwait(ConfigureAwaitOptions.None);
                return MenuResult.Ok;

            case 'c':
                ConfirmSteamScreenshotCleanup();
                return MenuResult.PrintMenu;

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

    private void ConfirmSteamScreenshotCleanup()
    {
        _writer.ConfirmScreenshotCleanup();
        for (var i = 0; i < 3; i++)
        {
            var pressedKey = Console.ReadKey(false);
            if (pressedKey.Key != ConsoleKey.Y)
            {
                _writer.WriteLine("");
                _writer.WriteInfo("Cancelled");
                return;
            }
            _writer.WriteLine("");
        }

        _computer.CleanupSteamScreenshotFile();
    }
}