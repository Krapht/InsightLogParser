using System.Linq;

namespace InsightLogParser.Client.Menu;

internal class CheeseMenu : IMenu
{
    private readonly Spider _spider;

    public CheeseMenu(Spider spider)
    {
        _spider = spider;
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
            default:
                return MenuResult.NotValidOption;
        }
    }
}