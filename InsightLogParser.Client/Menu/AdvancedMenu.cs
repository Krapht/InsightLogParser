using System.Linq;

namespace InsightLogParser.Client.Menu;

internal class AdvancedMenu : IMenu
{
    private readonly UserComputer _computer;
    private readonly MessageWriter _writer;
    private readonly Spider _spider;

    public AdvancedMenu(UserComputer computer, MessageWriter writer, Spider spider)
    {
        _computer = computer;
        _writer = writer;
        _spider = spider;
    }

    public IEnumerable<(char? key, string text)> MenuOptions
    {
        get
        {
            yield return (null, "Advanced or risky operations. Press [h] to display this menu again and backspace to back");
            yield return ('c', "Cleanup steam screenshot.vdf file (at your own risk)");
            if (_spider.IsOnline())
            {
                yield return ('u', "upload your offline progress to import solved puzzles");
            }
        }
    }

    public async Task<MenuResult> HandleOptionAsync(char keyChar)
    {
        switch (keyChar)
        {
            case 'c':
                ConfirmSteamScreenshotCleanup();
                return MenuResult.PrintMenu;
            case 'u':
                await ConfirmOfflineSaveUpload();
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

    private async Task ConfirmOfflineSaveUpload()
    {
        _writer.ConfirmOfflineSaveUpload();
        var pressedKey = Console.ReadKey(false);
        if (pressedKey.Key != ConsoleKey.Y)
        {
            _writer.WriteLine("");
            _writer.WriteInfo("Cancelled");
            return;
        }
        _writer.WriteLine("");

        await _spider.ImportSaveGameAsync();
    }
}