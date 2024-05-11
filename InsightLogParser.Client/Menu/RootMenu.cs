using System.Linq;

namespace InsightLogParser.Client.Menu;

internal class RootMenu : IMenu
{
    private readonly MenuHandler _menuHandler;
    private readonly Spider _spider;

    public IEnumerable<(char? key, string text)> MenuOptions { get; } = new (char? key, string text)[]
    {
        (null, "Available key commands. Press backspace to quit"),
        ('h', "Show this list again"),
        ('c', "--> Configuration [WIP]"),
        ('s', "--> Statistics"),
    };

    public RootMenu(MenuHandler menuHandler, Spider spider)
    {
        _menuHandler = menuHandler;
        _spider = spider;
    }

    public Task<bool> HandleOptionAsync(char keyChar)
    {
        switch (keyChar)
        {
            case 'c':
                _menuHandler.EnterMenu(new ConfigurationMenu(_spider));
                return Task.FromResult(true);
            case 's':
                _menuHandler.EnterMenu(new StatisticsMenu(_menuHandler, _spider));
                return Task.FromResult(true);
            default:
                return Task.FromResult(false);
        }
    }
}