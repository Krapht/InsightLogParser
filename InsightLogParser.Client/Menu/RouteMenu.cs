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
            yield return ('u', "Get route to unsolved local puzzles");
            if (_spider.IsOnline())
            {
                yield return ('f', "Fetch sighted unsolved puzzles for a zone from Cetus");
            }
            if (_spider.HasRoute())
            {
                yield return ('n', "Next waypoint");
                yield return ('p', "Previous waypoint");
                yield return ('c', "Clear route");
                if (_spider.IsOnline())
                {
                    yield return ('b', "Open puzzle in browser");
                }
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

            case 'f':
                if (!_spider.IsOnline()) return MenuResult.NotValidOption;
                var zone = _menuHandler.PickZone();
                if (zone == PuzzleZone.Unknown) return MenuResult.NotValidOption;
                await _spider.GenerateUnsolvedRouteFromCetus(zone);
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

            case 'b':
                if (!_spider.HasRoute() || !_spider.IsOnline()) return MenuResult.NotValidOption;
                _spider.OpenCurrentWaypointOnCetus();
                return MenuResult.Ok;

            default:
                return MenuResult.NotValidOption;
        }
    }
}