using System.Linq;
using InsightLogParser.Common.World;

namespace InsightLogParser.Client.Menu;

internal class StatisticsMenu : IMenu
{
    private readonly MenuHandler _menuHandler;
    private readonly Spider _spider;

    public StatisticsMenu(MenuHandler menuHandler, Spider spider)
    {
        _menuHandler = menuHandler;
        _spider = spider;
    }

    public IEnumerable<(char? key, string text)> MenuOptions
    {
        get
        {
            yield return (null, "Select the zone, Press backspace to go back");
            yield return ('h', "Show this list again");
            if (_spider.IsOnline())
            {
                yield return ('p', "Unparsed puzzle sightings from Cetus");
                yield return ('P', "All puzzle sightings from Cetus");
            }
            yield return ('1', "Verdant Glen");
            yield return ('2', "Lucent Waters");
            yield return ('3', "Autumn Falls");
            yield return ('4', "Shady Wildwood");
            yield return ('5', "Serene Deluge");
        }
    }

    private async Task<bool> HandleSightings(bool filtered)
    {
        if (!_spider.IsOnline()) return false;
        var sightZone = _menuHandler.PickZone();
        if (sightZone == PuzzleZone.Unknown) return true;
        var sightType = _menuHandler.PickWorldPuzzleType();
        if (sightType == PuzzleType.Unknown) return true;
        await _spider.WriteSightingsAsync(sightZone, sightType, filtered);
        return true;
    }

    public async Task<MenuResult> HandleOptionAsync(char keyChar)
    {
        switch (keyChar)
        {
            case 'p':
                return await HandleSightings(true) ? MenuResult.Ok : MenuResult.NotValidOption;
            case 'P':
                return await HandleSightings(false) ? MenuResult.Ok : MenuResult.NotValidOption;
            case '1':
                await _spider.WriteStatistics(PuzzleZone.VerdantGlen);
                return MenuResult.Ok;
            case '2':
                await _spider.WriteStatistics(PuzzleZone.LucentWaters);
                return MenuResult.Ok;
            case '3':
                await _spider.WriteStatistics(PuzzleZone.AutumnFalls);
                return MenuResult.Ok;
            case '4':
                await _spider.WriteStatistics(PuzzleZone.ShadyWildwood);
                return MenuResult.Ok;
            case '5':
                await _spider.WriteStatistics(PuzzleZone.SereneDeluge);
                return MenuResult.Ok;
            default:
                return MenuResult.NotValidOption;
        }
    }
}