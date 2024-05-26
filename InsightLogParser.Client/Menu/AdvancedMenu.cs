using System.Linq;

namespace InsightLogParser.Client.Menu;

internal class AdvancedMenu : IMenu
{
    private readonly MenuHandler _menuHandler;
    private readonly UserComputer _computer;
    private readonly MessageWriter _writer;

    public AdvancedMenu(MenuHandler menuHandler, UserComputer computer, MessageWriter writer)
    {
        _menuHandler = menuHandler;
        _computer = computer;
        _writer = writer;
    }

    public IEnumerable<(char? key, string text)> MenuOptions
    {
        get
        {
            yield return (null, "Advanced or risky operations. Press [h] to display this menu again and backspace to back");
            yield return ('c', "Cleanup steam screenshot.vdf file (at your own risk)");
        }
    }

    public Task<MenuResult> HandleOptionAsync(char keyChar)
    {
        switch (keyChar)
        {
            case 'c':
                ConfirmSteamScreenshotCleanup();
                return Task.FromResult(MenuResult.PrintMenu);
            default:
                return Task.FromResult(MenuResult.NotValidOption);
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