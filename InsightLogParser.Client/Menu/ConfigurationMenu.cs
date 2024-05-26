using System.Linq;

namespace InsightLogParser.Client.Menu;

internal class ConfigurationMenu : IMenu
{
    private readonly Spider _spider;

    public ConfigurationMenu(Spider spider)
    {
        _spider = spider;
    }

    public IEnumerable<(char? key, string text)> MenuOptions => GenerateMenu();

    private IEnumerable<(char? key, string text)> GenerateMenu()
    {
        var currentConfig = _spider.GetCurrentConfiguration();
        return new (char? key, string text)[]
        {
            (null, "Configuration [WIP]. Press [h] to display this menu again and backspace to back"),
            ('d', $"Toggle {nameof(Configuration.DebugMode)}, current value {currentConfig.DebugMode}"),
            ('r', $"Toggle {nameof(Configuration.ShowGameLogLines)}, current value {currentConfig.ShowGameLogLines}"),
        };
    }

    public Task<MenuResult> HandleOptionAsync(char keyChar)
    {
        var currentConfig = _spider.GetCurrentConfiguration();
        switch (keyChar)
        {
            case 'd':
                currentConfig.DebugMode = !currentConfig.DebugMode;
                return Task.FromResult(MenuResult.PrintMenu);
            case 'r':
                currentConfig.ShowGameLogLines = !currentConfig.ShowGameLogLines;
                return Task.FromResult(MenuResult.PrintMenu);
        }

        return Task.FromResult(MenuResult.NotValidOption);
    }
}