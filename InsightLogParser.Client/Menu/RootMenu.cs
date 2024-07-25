using System.Diagnostics;

namespace InsightLogParser.Client.Menu;

internal class RootMenu : IMenu
{
    private readonly MenuHandler _menuHandler;
    private readonly Spider _spider;
    private readonly MessageWriter _writer;
    private readonly UserComputer _computer;

    public IEnumerable<(char? key, string text)> MenuOptions
    {
        get
        {
            yield return (null, "Available key commands. Press backspace to quit");
            yield return ('h', "Show this list again");
            if (_spider.IsOnline())
            {
                yield return ('w', "Log-in to cetus web ui");
            }
            yield return ('U', "--> Open the UI [WIP]");
            yield return ('c', "--> Configuration [WIP]");
            yield return ('s', "--> Statistics");
            yield return ('r', "--> Routes");
            yield return ('a', "--> Advanced or risky");
            if (_spider.IsOnline())
            {
                yield return ('C', "--> Che....ese (online only)");
            }
        }
    }

    public RootMenu(MenuHandler menuHandler, Spider spider, MessageWriter writer, UserComputer computer)
    {
        _menuHandler = menuHandler;
        _spider = spider;
        _writer = writer;
        _computer = computer;
    }

    public async Task<MenuResult> HandleOptionAsync(char keyChar)
    {
        switch (keyChar)
        {
            case 'w':
                await _spider.OpenCetusWebAsync().ConfigureAwait(ConfigureAwaitOptions.None);
                return MenuResult.Ok;
            case 'c':
                _menuHandler.EnterMenu(new ConfigurationMenu(_spider));
                return MenuResult.Ok;
            case 's':
                _menuHandler.EnterMenu(new StatisticsMenu(_menuHandler, _spider));
                return MenuResult.Ok;
            case 'r':
                _menuHandler.EnterMenu(new RouteMenu(_menuHandler, _spider));
                return MenuResult.Ok;
            case 'a':
                _menuHandler.EnterMenu(new AdvancedMenu(_computer, _writer, _spider));
                return MenuResult.Ok;
            case 'C':
                _menuHandler.EnterMenu(new CheeseMenu(_spider, _writer));
                return MenuResult.Ok;
            case 'U':
                if (Process.GetProcessesByName("InsightLogParser.UI").Length != 0)
                {
                    _writer.WriteDebug("UI is already running");
                }
                else
                {
                    Process.Start("InsightLogParser.UI.exe");
                }
                
                return MenuResult.Ok;
            default:
                return MenuResult.NotValidOption;
        }
    }
}