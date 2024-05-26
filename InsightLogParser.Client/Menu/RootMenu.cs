using System.Linq;

namespace InsightLogParser.Client.Menu;

internal class RootMenu : IMenu
{
    private readonly MenuHandler _menuHandler;
    private readonly Spider _spider;
    private readonly MessageWriter _writer;
    private readonly UserComputer _computer;

    public IEnumerable<(char? key, string text)> MenuOptions { get; } = new (char? key, string text)[]
    {
        (null, "Available key commands. Press backspace to quit"),
        ('h', "Show this list again"),
        ('c', "--> Configuration [WIP]"),
        ('s', "--> Statistics"),
        ('a', "--> Advanced or risky")
    };

    public RootMenu(MenuHandler menuHandler, Spider spider, MessageWriter writer, UserComputer computer)
    {
        _menuHandler = menuHandler;
        _spider = spider;
        _writer = writer;
        _computer = computer;
    }

    public Task<MenuResult> HandleOptionAsync(char keyChar)
    {
        switch (keyChar)
        {
            case 'c':
                _menuHandler.EnterMenu(new ConfigurationMenu(_spider));
                return Task.FromResult(MenuResult.Ok);
            case 's':
                _menuHandler.EnterMenu(new StatisticsMenu(_menuHandler, _spider));
                return Task.FromResult(MenuResult.Ok);
            case 'a':
                _menuHandler.EnterMenu(new AdvancedMenu(_menuHandler, _computer, _writer, _spider));
                return Task.FromResult(MenuResult.Ok);
            default:
                return Task.FromResult(MenuResult.NotValidOption);
        }
    }
}