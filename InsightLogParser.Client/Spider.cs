
using InsightLogParser.Client.Cetus;
using InsightLogParser.Client.Screenshots;
using InsightLogParser.Common;
using InsightLogParser.Common.ApiModels;
using InsightLogParser.Common.PuzzleParser;
using InsightLogParser.Common.Screenshots;
using InsightLogParser.Common.World;

namespace InsightLogParser.Client
{
    internal class Spider
    {
        private readonly MessageWriter _messageWriter;
        private readonly Beeper _beeper;
        private readonly Configuration _configuration;
        private readonly ParsedDatabase _db;
        private readonly ICetusClient _cetusClient;
        private readonly TimeTools _timeTools;
        private readonly GamePuzzleHandler _gamePuzzleHandler;
        private readonly UserComputer _computer;
        private ScreenshotManager? _screenshotManager = null;

        //State
        private DateTimeOffset? _sessionStart;
        private string? _serverAddress;

        public Spider(MessageWriter messageWriter
            , Configuration configuration
            , ParsedDatabase db
            , ICetusClient cetusClient
            , TimeTools timeTools
            , GamePuzzleHandler gamePuzzleHandler
            , UserComputer computer
            )
        {
            _messageWriter = messageWriter;
            _beeper = new Beeper(configuration);
            _configuration = configuration;
            _db = db;
            _cetusClient = cetusClient;
            _timeTools = timeTools;
            _gamePuzzleHandler = gamePuzzleHandler;
            _computer = computer;
        }

        public void SetScreenshotManager(ScreenshotManager screenshotManager)  { _screenshotManager = screenshotManager; }

        public void StartSession(DateTimeOffset timestamp)
        {
            _sessionStart = timestamp;
            _messageWriter.SessionStarted(timestamp);
        }

        public void SetServer(string serverAddress)
        {
            _serverAddress = serverAddress;
            _messageWriter.ConnectedToServer(serverAddress);
        }


        public void EndSession(DateTimeOffset timestamp)
        {
            _sessionStart = null;
            _serverAddress = null;
            _messageWriter.SessionEnded(timestamp);
        }

        public async Task SessionBeingDisconnectedAsync()
        {
            _messageWriter.SessionDisconnecting();
            await _beeper.BeepForAttentionAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        }

        public async Task SolvedAsync(DateTimeOffset timestamp, int puzzleId, PuzzleZone zone, PuzzleType type, short? difficulty)
        {
            if (!ShouldHandlePuzzle(puzzleId, zone, type)) return;

            //Local
            var lastSolved = _db.GetLastSolved(zone, type, puzzleId);
            _db.AddSolved(zone, type, puzzleId, timestamp);

            if (!WorldInformation.Zones.TryGetValue(zone, out var worldZone)) return;
            if (!worldZone.Puzzles.TryGetValue(type, out var worldPuzzle)) return;
            var zoneName = WorldInformation.GetZoneName(zone);
            var puzzleName = WorldInformation.GetPuzzleName(type);
            var timeToCycle = _timeTools.GetTimeTo(worldPuzzle.CycleTime);
            var lastResetTime = _timeTools.GetCurrentCycleStartTime(worldPuzzle.CycleTime);
            var isFresh = lastResetTime < _sessionStart;
            var (parsedCount, cycleCount, totalCount) = _db.GetParsedCount(zone, type, puzzleId, lastResetTime);

            var solvedAgo = lastSolved.HasValue ? _timeTools.GetTimeSince(lastSolved.Value) : (TimeSpan?)null;

            _messageWriter.WriteParsed(zoneName, puzzleName, puzzleId, parsedCount, cycleCount, totalCount, worldPuzzle.TotalPuzzles, timeToCycle, solvedAgo);

            //Online
            var cetusTask = _cetusClient.PostSolvedAsync(new PlayerReport()
            {
                PuzzleId = puzzleId,
                Type = type,
                Zone = zone,
                Timestamp = timestamp,
                IsFresh = isFresh,
                ServerAddress = _serverAddress,
            });

            if (lastSolved.HasValue)
            {
                _beeper.BeepForKnownParse();
            }
            else
            {
                _beeper.BeepForNewParse();
            }

            var cetusInfo = await cetusTask;
            if (cetusInfo?.Sightings != null)
            {
                _messageWriter.WriteDebug("Got result from Cetus");
                var sightingsCount = cetusInfo.Sightings.Length;
                var solved = _db.GetSolvedIds(zone, type);
                var unsolvedIds = solved.Except(cetusInfo.Sightings.Select(x => x.PuzzleId));
                var unsolved = cetusInfo.Sightings.Join(unsolvedIds, x => x.PuzzleId, x => x, (sighting, _) => sighting)
                    .ToList();

                var needsScreenshots = _screenshotManager?.GetScreenshotStatus(type, true, cetusInfo.ScreenshotCategories) ?? [];
                var screenShotStrings = needsScreenshots
                    .Select(x => (CategoryName: ScreenshotManager.GetCategoryName(x.Category), IsRequested: x.IsMissing))
                    .ToList();
                _messageWriter.WriteCetusParsed(sightingsCount, unsolved.Count(x => x.IsFresh), unsolved.Count(x => !x.IsFresh), screenShotStrings);
                if (screenShotStrings.Any(x => x.IsRequested))
                {
                    _beeper.BeepForMissingScreenshot();
                }
            }

            _messageWriter.WriteEndSolved();

            _screenshotManager?.SetLastPuzzle(puzzleId, true);
        }

        public async Task OpenedAsync(DateTimeOffset timestamp, int puzzleId, PuzzleZone zone, PuzzleType type, short? difficulty)
        {
            if (!ShouldHandlePuzzle(puzzleId, zone, type)) return;

            //Local
            var lastSolved = _db.GetLastSolved(zone, type, puzzleId);
            var agoTime = lastSolved.HasValue ? _timeTools.GetTimeSince(lastSolved.Value) : default(TimeSpan?);
            if (!WorldInformation.Zones.TryGetValue(zone, out var worldZone)) return;
            if (!worldZone.Puzzles.TryGetValue(type, out var worldPuzzle)) return;
            var zoneName = WorldInformation.GetZoneName(zone);
            var puzzleName = WorldInformation.GetPuzzleName(type);

            _messageWriter.WriteOpened(zoneName, puzzleName, puzzleId, agoTime);

            var cycleStartTime = _timeTools.GetCurrentCycleStartTime(worldPuzzle.CycleTime);
            var isFresh = cycleStartTime < _sessionStart;

            //Online
            var cetusTask = _cetusClient.PostSeenAsync(new PlayerReport()
            {
                PuzzleId = puzzleId,
                Type = type,
                Zone = zone,
                Timestamp = timestamp,
                IsFresh = isFresh,
                ServerAddress = _serverAddress,
            });
            if (lastSolved.HasValue)
            {
                _beeper.BeepForOpeningSolvedPuzzle();
            }
            var seenResponse = await cetusTask.ConfigureAwait(ConfigureAwaitOptions.None);
            if (seenResponse != null && _screenshotManager != null)
            {
                var screenshotStatus = _screenshotManager.GetScreenshotStatus(type, false, seenResponse.Screenshots);
                var statusStrings = screenshotStatus
                    .Select(x => (CategoryName: ScreenshotManager.GetCategoryName(x.Category), IsRequested: x.IsMissing))
                    .ToList();

                _messageWriter.WriteCetusOpened(statusStrings);
                if (statusStrings.Any(x => x.IsRequested))
                {
                    _beeper.BeepForMissingScreenshot();
                }
            }
            _messageWriter.WriteEndOpened();
            _screenshotManager?.SetLastPuzzle(puzzleId, false);
        }

        private bool ShouldHandlePuzzle(int puzzleId, PuzzleZone zone, PuzzleType type)
        {
            if (ShouldSkipZone(zone))
            {
                _messageWriter.WriteDebug($"Skipping puzzle in {WorldInformation.GetZoneName(zone)}");
                return false;
            }

            if (ShouldSSkipPuzzleType(type))
            {
                _messageWriter.WriteDebug($"Skipping {WorldInformation.GetPuzzleName(type)} puzzle");
                return false;
            }

            var isWorldPuzzle = _gamePuzzleHandler.IsWorldPuzzle(puzzleId);
            if (isWorldPuzzle == null)
            {
                _messageWriter.WriteDebug("No puzzle.json found, everything is a world puzzle");
                return true;
            }
            if (isWorldPuzzle == false)
            {
                _messageWriter.WriteDebug("Puzzle was not a world puzzle and will be skipped");
                return false;
            }

            return true;
        }

        public async Task ClosedAsync(DateTimeOffset timestamp, int puzzleId, PuzzleZone zone, PuzzleType type, short? difficulty)
        {
            await Task.Yield();
        }

        private bool ShouldSkipZone(PuzzleZone zone)
        {
            switch (zone)
            {
                case PuzzleZone.Unknown:
                    return true;
                default: return false;
            }
        }

        private bool ShouldSSkipPuzzleType(PuzzleType type)
        {
            switch (type)
            {
                case PuzzleType.Unknown:
                case PuzzleType.ArmillaryRings:
                case PuzzleType.Skydrop:
                    return true;
                default:
                    return false;
            }
        }

        public void WriteRawLogLine(string line)
        {
            if (_configuration.ShowGameLogLines)
            {
                _messageWriter.WriteLogLine(line);
            }
        }

        public Configuration GetCurrentConfiguration()
        {
            return _configuration;
        }

        public async Task WriteStatistics(PuzzleZone zone)
        {
            var puzzles = WorldInformation.Zones[zone].Puzzles.Values.OrderBy(x => x.Order)
                .Select(x => (Puzzle: x, PuzzleName: WorldInformation.GetPuzzleName(x.PuzzleType)))
                .ToList();
            var zoneName = WorldInformation.GetZoneName(zone);

            var cetusStats = await _cetusClient.GetZoneStatisticsAsync(zone);

            var puzzleStatistics = puzzles.Select(puzzleTuple =>
            {
                var puzzle = puzzleTuple.Puzzle;
                var resetTime = puzzle.CycleTime;
                var lastReset = _timeTools.GetCurrentCycleStartTime(resetTime);
                var timeToCycle = _timeTools.GetTimeTo(resetTime).ToHHMMSS(true);
                var total = puzzle.TotalPuzzles;
                var solves = _db.GetSolves(zone, puzzle.PuzzleType);
                var cycleSolved = solves?.Where(s => s.LastSolved > lastReset).Count() ?? 0;
                var totalSolved = solves?.Count ?? 0;

                var fromCetus = cetusStats?.Puzzles
                    .Where(c => c.PuzzleType == puzzle.PuzzleType)
                    .Select(stat =>
                    {
                        var sightingsCount = stat.Sightings.Count;
                        var solved = _db.GetSolvedIds(zone, stat.PuzzleType);
                        var unsolvedIds = solved.Except(stat.Sightings.Select(s => s.PuzzleId));
                        var unsolved = stat.Sightings.Join(unsolvedIds, x => x.PuzzleId, x => x, (sighting, _) => sighting)
                            .ToList();
                        return new MessageWriter.CetusStatistics
                        {
                            Unsolved = unsolved.Count(c => c.IsFresh),
                            Stale = unsolved.Count(c => !c.IsFresh),
                            Total = sightingsCount,
                            Expected =  stat.ExpectedCount,
                        };
                    }).FirstOrDefault();

                return new MessageWriter.Statistics
                {
                    PuzzleName =  puzzleTuple.PuzzleName,
                    CycleSolved =  cycleSolved,
                    TotalSolved = totalSolved,
                    Total = total,
                    TimeToCycle = timeToCycle,
                    CetusStats = fromCetus
                };
            }).ToList();

            _messageWriter.WriteStatistics(zoneName, puzzleStatistics, cetusStats == null);
        }

        public bool IsOnline()
        {
            return !_cetusClient.IsDummy();
        }

        public async Task WriteSightingsAsync(PuzzleZone zone, PuzzleType type, bool filtered)
        {
            var sightings = await _cetusClient.GetSightingsAsync(zone, type).ConfigureAwait(ConfigureAwaitOptions.None);
            if (sightings == null) return;

            if (filtered)
            {
                var solved = _db.GetSolvedIds(zone, type).ToHashSet();
                sightings = sightings.Where(x => !solved.Contains(x.PuzzleId)).ToArray();
            }

            var zoneName = WorldInformation.GetZoneName(zone);
            var puzzleName = WorldInformation.GetPuzzleName(type);

            var entries = sightings
                .OrderBy(x => x.PuzzleId)
                .Select(x => (PuzzleId: x.PuzzleId, Stale: !x.IsFresh, Servers: x.ServerAddresses));

            _messageWriter.WriteSightings(zoneName, puzzleName, entries, _serverAddress!);
        }

        public async Task<bool> UploadScreenshotAsync(CapturedScreenshot capturedScreenshot, ScreenshotCategory screenshotCategory)
        {
            var screenshotPath = capturedScreenshot.ScreenshotPath;
            var puzzleId = capturedScreenshot.PuzzleId;

            if (!File.Exists(screenshotPath)) return false;

            var fileTask = File.ReadAllBytesAsync(screenshotPath);
            var folder = Path.GetDirectoryName(screenshotPath) ?? ".";
            var fileName = Path.GetFileName(screenshotPath);
            var thumbPath = Path.Combine(folder, "thumbnails");
            var thumbFile = Path.Combine(thumbPath, fileName);
            var thumbBytes = File.Exists(thumbFile)
                ? await File.ReadAllBytesAsync(thumbFile).ConfigureAwait(ConfigureAwaitOptions.None)
                : null;
            var screenshotBytes = await fileTask.ConfigureAwait(ConfigureAwaitOptions.None);

            var screenshot = new Screenshot()
            {
                PuzzleId = puzzleId,
                Category = screenshotCategory,
                ScreenshotBytes = screenshotBytes,
                ThumbBytes = thumbBytes,
            };

            var result = await _cetusClient.PostScreenshotAsync(screenshot).ConfigureAwait(ConfigureAwaitOptions.None);

            var success = result != null;

            if (success)
            {
                _screenshotManager?.FlagScreenshotAsHandled();
            }

            return success ;
        }

        public ScreenshotManager? GetScreenshotManager()
        {
            return _screenshotManager;
        }


        public async Task OpenCetusWebAsync()
        {
            var code = await _cetusClient.GetSigninCodeAsync();
            _computer.LaunchBrowser($"{_configuration.CetusUri}/account/signin?code={code}");
        }
    }
}
