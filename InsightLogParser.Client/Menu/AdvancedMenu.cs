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
            yield return ('w', "Log-in to cetus web ui");
            yield return ('c', "Cleanup steam screenshot.vdf file (at your own risk)");
        }
    }

    public async Task<MenuResult> HandleOptionAsync(char keyChar)
    {
        switch (keyChar)
        {
            case 'w':
                await _spider.OpenCetusWebAsync();
                return MenuResult.Ok;
            case 'c':
                ConfirmSteamScreenshotCleanup();
                return MenuResult.PrintMenu;
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