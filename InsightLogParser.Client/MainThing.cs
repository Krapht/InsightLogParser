using InsightLogParser.Client.Cetus;
using InsightLogParser.Client.Menu;
using InsightLogParser.Client.Parsing;
using InsightLogParser.Client.Routing;
using InsightLogParser.Client.Screenshots;
using InsightLogParser.Common;
using InsightLogParser.Common.PuzzleParser;
using InsightLogParser.Common.World;

namespace InsightLogParser.Client;

public class MainThing
{
    private const string ConfigurationFilename = "configuration.json";

    private readonly CancellationToken _forcedExitToken;
    private readonly MessageWriter _messageWriter;

    private ConfigurationManager _configurationManager = null!;
    private GamePuzzleHandler _puzzleHandler = null!;
    private ParsedDatabase _db = null!;
    private ICetusClient _apiClient = null!;
    private readonly TimeTools _timeTools;
    private ScreenshotMonitor? _screenshotMonitor = null;

    internal ICetusClient CetusClient {
        get => _apiClient;
    }

    public MainThing(CancellationToken forcedExitToken)
    {
        _forcedExitToken = forcedExitToken;
        _messageWriter = new MessageWriter();
        _timeTools = new TimeTools(TimeProvider.System);
    }
    public async Task RunAsync()
    {
        //Initialize
        var configuration = await InitializeConfigurationAsync();

        var computer = new UserComputer(configuration, _messageWriter);
        var logFilePath = computer.GetPrimaryLogFile();
        var logFileFolder = Path.GetDirectoryName(logFilePath);
        if (string.IsNullOrWhiteSpace(logFileFolder)) logFileFolder = ".";
        if (!Directory.Exists(logFileFolder))
        {
            _messageWriter.WriteInitLine($"Failed to locate log file folder '{logFileFolder}'", ConsoleColor.Red);
            return;
        }

        await InitializeParsedDbAsync(configuration);
        var matchExpected =  await InitializePuzzleHandlerAsync(computer, _db);
        if (matchExpected)
        {
            _db.RemoveNonWorldPuzzles(_puzzleHandler);
        }

        var cycleTimer = await CycleTimer.CreateAndStartAsync(_messageWriter, _timeTools).ConfigureAwait(ConfigureAwaitOptions.None);

        await InitializeCetusClientAsync(configuration);

        var teleportManager = new TeleportManager(_messageWriter);

        _messageWriter.WriteInitLine("Poking spider", ConsoleColor.Green);
        var puzzleIterator = new PuzzleRouter(_messageWriter, configuration);
        var serverTracker = new ServerTracker(_messageWriter);
        var spider = new Spider(_messageWriter, configuration, _db, _apiClient, _timeTools, _puzzleHandler, computer, teleportManager, puzzleIterator, serverTracker);

        //Screenshots only makes sense in online mode
        if (spider.IsOnline() && configuration.MonitorScreenshots)
        {
            var screenshotManager = new ScreenshotManager(_puzzleHandler, _messageWriter, computer);
            spider.SetScreenshotManager(screenshotManager);
            var screenshotFolder = computer.GetScreenshotFolder();
            if (!Directory.Exists(screenshotFolder))
            {
                _messageWriter.WriteInitLine($"Screenshot folder '{screenshotFolder}' does not exist", ConsoleColor.Red);
            }
            else
            {
                _messageWriter.WriteInitLine($"Monitoring screenshot folder '{screenshotFolder}'", ConsoleColor.Green);
                _screenshotMonitor = new ScreenshotMonitor(screenshotFolder, screenshotManager);
            }
        }

        _messageWriter.WriteInitLine($"Using '{logFilePath}' as the log file", ConsoleColor.Green);
        _messageWriter.WriteInitLine("Starting log processor", ConsoleColor.Green);
        var processor = new LogProcessor(_messageWriter, computer, _forcedExitToken, spider);
        await processor.StartAsync(logFilePath);

        _messageWriter.WriteWelcome();

        var menu = new MenuHandler(_forcedExitToken, _messageWriter, spider, configuration, computer);
        await menu.StartAsync().ConfigureAwait(ConfigureAwaitOptions.None);

        //Fully started

        await menu.WaitForMenuClosed().ConfigureAwait(ConfigureAwaitOptions.None);

        //Teardown
        _screenshotMonitor?.Dispose();

        await cycleTimer.StopAsync();
        await _db.StopAsync();
        await processor.StopAsync();
    }

    private async Task<Configuration> InitializeConfigurationAsync()
    {
        _configurationManager = new ConfigurationManager(ConfigurationFilename, _messageWriter);
        var configuration = await _configurationManager.LoadConfigurationAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        _configurationManager.ValidateConfiguration(configuration);
        _messageWriter.SetConfiguration(configuration);
        return configuration;
    }

    private async Task<bool> InitializePuzzleHandlerAsync(UserComputer computer, ParsedDatabase db)
    {
        _puzzleHandler = new GamePuzzleHandler();

        var jsonPath = computer.GetPuzzleJsonPath();
        _messageWriter.WriteInitLine($"Using '{jsonPath}' as puzzle json file", ConsoleColor.Green);

        const string unavailable = "Some features may be unavailable";
        if (jsonPath == null)
        {
            _messageWriter.WriteInitLine($"Unable to locate Puzzle.json file. {unavailable}", ConsoleColor.Red);
            return false;
        }
        if (!File.Exists(jsonPath))
        {
            _messageWriter.WriteInitLine($"Expected file '{jsonPath}' did not exist. {unavailable}", ConsoleColor.Red);
            return false;
        }

        try
        {
            await _puzzleHandler.LoadAsync(jsonPath);
        }
        catch (Exception ex)
        {
            _messageWriter.WriteInitLine($"Unexpected error occured when reading the puzzle file. {unavailable}", ConsoleColor.Red);
            _messageWriter.WriteDebug(ex.ToString());
            return false;
        }

        if (!_puzzleHandler.PuzzleDatabase.Keys.Any())
        {
            _messageWriter.WriteInitLine($"Puzzle.json could not be loaded correctly. {unavailable}", ConsoleColor.Red);
            return false;
        }

        //Check puzzle counts
        bool unexpected = false;

        //TODO: Figure out how the count is split across the zones
        var countTotals = new[]
        {
            PuzzleType.SentinelStones,
            PuzzleType.CrystalLabyrinth,
            PuzzleType.MatchThree,
            PuzzleType.RollingBlock,
            PuzzleType.PhasicDial,
            PuzzleType.MorphicFractal,
            PuzzleType.ShiftingMosaic,
        };

        foreach (var zone in Enum.GetValues<PuzzleZone>())
        {
            if (zone == PuzzleZone.Unknown) continue;
            if (!WorldInformation.Zones.TryGetValue(zone, out var zoneInfo)) continue;
            foreach (var type in Enum.GetValues<PuzzleType>()
                         .Where(x => !countTotals.Contains(x)))
            {
                if (type == PuzzleType.Unknown) continue;
                if (!zoneInfo.Puzzles.TryGetValue(type, out var puzzleInfo)) continue;

                var expected = puzzleInfo.TotalPuzzles;
                var actual = _puzzleHandler.PuzzleDatabase.Values
                    .Count(x => x.IsWorldPuzzle &&
                                x.Zone == zone && x.Type == type);
                if (expected != actual)
                {
                    unexpected = true;
                    _messageWriter.WriteInitLine($"{WorldInformation.GetZoneName(zone)} Puzzle.json count for {WorldInformation.GetPuzzleName(type)} was {actual}, expected {expected}", ConsoleColor.Yellow);
                }
            }
        }

        foreach (var totalsOnly in countTotals)
        {
            var expected = WorldInformation.Zones.Values
                .SelectMany(x => x.Puzzles.Values)
                .Where(x => x.PuzzleType == totalsOnly)
                .Sum(x => x.TotalPuzzles);

            var actual = _puzzleHandler.PuzzleDatabase.Values
                .Count(x => x.IsWorldPuzzle && x.Type == totalsOnly);
            if (expected != actual)
            {
                unexpected = true;
                _messageWriter.WriteInitLine($"Total Puzzle.json count for {WorldInformation.GetPuzzleName(totalsOnly)} was {actual}, expected {expected}", ConsoleColor.Yellow);
            }
            _messageWriter.WriteDebug($"Reminder: Still missing zone count for {WorldInformation.GetPuzzleName(totalsOnly)}");
        }

        if (unexpected)
        {
            return false;
        }

        _messageWriter.WriteInitLine("Puzzle counts in Puzzle.json all match expected values", ConsoleColor.Green);
        return true;
    }

    private async Task InitializeParsedDbAsync(Configuration configuration)
    {
        var dbPath = !string.IsNullOrWhiteSpace(configuration.ForcedParsedDatabasePath) ? configuration.ForcedParsedDatabasePath : "db.json";
        _db = new ParsedDatabase(dbPath, _messageWriter, _forcedExitToken);
        await _db.StartAsync();
    }

    private async Task InitializeCetusClientAsync(Configuration configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration.CetusUri) || string.IsNullOrWhiteSpace(configuration.CetusApiKey) || configuration.CetusPlayerId == null || !configuration.CetusUri.StartsWith("https://"))
        {
            _messageWriter.WriteInitLine("Cetus configuration missing, staying in offline mode", ConsoleColor.Green);
            _apiClient = new DummyCetusClient();
            return;
        }

        var url = configuration.CetusUri;
        var playerId = configuration.CetusPlayerId.Value;
        var apiKey = configuration.CetusApiKey;

        _messageWriter.WriteInitLine("Cetus configuration detected, switching to online mode", ConsoleColor.Green);
        _apiClient = new CetusClient(url, _messageWriter);

        _messageWriter.WriteDebug($"Using player id {playerId:D}");
        var couldAuthenticate = await _apiClient.AuthenticateAsync(playerId, apiKey).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!couldAuthenticate)
        {
            _messageWriter.WriteInitLine("Failed to authenticate with Cetus, falling back to offline mode", ConsoleColor.Red);
            _apiClient.Dispose();
            _apiClient = new DummyCetusClient();
        }
        _messageWriter.WriteInitLine($"Successfully authenticated with Cetus using player id {playerId:D}", ConsoleColor.Green);
    }
}