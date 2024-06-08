using System.Linq;
using InsightLogParser.Common.World;

namespace InsightLogParser.Client.Menu;

internal class RouteMenu : IMenu
{
    private readonly MenuHandler _menuHandler;
    private readonly Spider _spider;

    public RouteMenu(MenuHandler menuHandler, Spider spider)
    {
        _menuHandler = menuHandler;
        _spider = spider;
    }

    public IEnumerable<(char? key, string text)> MenuOptions
    {
        get
        {
            yield return (null, "Routes. Press [h] to display this menu again and backspace to back");
            yield return ('u', "Get route to unsolved puzzles");
            if (_spider.HasRoute())
            {
                yield return ('n', "Next waypoint");
                yield return ('p', "Previous waypoint");
                yield return ('c', "Clear route");
            }
        }
    }

    public async Task<MenuResult> HandleOptionAsync(char keyChar)
    {
        switch (keyChar)
        {
            case 'u':
                var puzzleZone = _menuHandler.PickZone();
                if (puzzleZone == PuzzleZone.Unknown) return MenuResult.NotValidOption;
                var puzzleType = _menuHandler.PickWorldPuzzleType();
                if (puzzleType == PuzzleType.Unknown) return MenuResult.NotValidOption;
                _spider.GenerateUnsolvedWaypoints(puzzleZone, puzzleType);
                return MenuResult.PrintMenu;

            case 'n':
                if (!_spider.HasRoute()) return MenuResult.NotValidOption;
                _spider.NextRouteWaypoint();
                return MenuResult.Ok;

            case 'p':
                if (!_spider.HasRoute()) return MenuResult.NotValidOption;
                _spider.PreviousRouteWaypoint();
                return MenuResult.Ok;

            case 'c':
                if (!_spider.HasRoute()) return MenuResult.NotValidOption;
                _spider.ClearRoute();
                return MenuResult.PrintMenu;

            default:
                return MenuResult.NotValidOption;
        }
    }
}