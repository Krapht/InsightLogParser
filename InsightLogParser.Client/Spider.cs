using InsightLogParser.Client.Cetus;
using InsightLogParser.Client.Routing;
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
        private readonly TeleportManager _teleportManager;
        private readonly PuzzleRouter _puzzleRouter;
        private readonly ServerTracker _serverTracker;
        private ScreenshotManager? _screenshotManager = null;

        //State
        private DateTimeOffset? _sessionStart;
        private string? _serverAddress;
        private int? _lastOpenedWithSolution;

        public Spider(MessageWriter messageWriter
            , Configuration configuration
            , ParsedDatabase db
            , ICetusClient cetusClient
            , TimeTools timeTools
            , GamePuzzleHandler gamePuzzleHandler
            , UserComputer computer
            , TeleportManager teleportManager
            , PuzzleRouter puzzleRouter
            , ServerTracker serverTracker
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
            _teleportManager = teleportManager;
            _puzzleRouter = puzzleRouter;
            _serverTracker = serverTracker;
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
            _serverTracker.Connected(serverAddress);
            _messageWriter.ConnectedToServer(serverAddress);
        }

        public void ConnectingToServer(string serverAddress)
        {
            _serverTracker.Connect(serverAddress);
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

            _messageWriter.WriteParsed(zoneName, puzzleName, puzzleId, parsedCount, cycleCount, totalCount, worldPuzzle.TotalPuzzles, timeToCycle, solvedAgo, !isFresh);
            if (!isFresh && _configuration.BeepForAttentionOnStaleSolve)
            {
                await _beeper.BeepForAttentionAsync();
            }

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
                var unsolvedIds = cetusInfo.Sightings.Select(x => x.PuzzleId).Except(solved);
                var unsolved = cetusInfo.Sightings.Join(unsolvedIds, x => x.PuzzleId, x => x, (sighting, _) => sighting)
                    .ToList();

                var needsScreenshots = _screenshotManager?.GetScreenshotStatus(type, true, cetusInfo.ScreenshotCategories) ?? [];
                var screenShotStrings = needsScreenshots
                    .Select(x => (CategoryName: ScreenshotManager.GetCategoryName(x.Category), IsRequested: x.IsMissing))
                    .ToList();
                _messageWriter.WriteCetusParsed(sightingsCount, unsolved.Count(x => x.IsFresh), unsolved.Count(x => !x.IsFresh), screenShotStrings, cetusInfo.FirstSighting);
                if (screenShotStrings.Any(x => x.IsRequested))
                {
                    _beeper.BeepForMissingScreenshot();
                }
            }

            _messageWriter.WriteEndSolved();

            // If we solved a puzzle, we're obviously at that puzzle, so let's move to it if we have a coordinate for it.
            var puzzle = _gamePuzzleHandler.PuzzleDatabase.Values.FirstOrDefault(p => p.KrakenId == puzzleId);
            if (puzzle != null && puzzle.PrimaryCoordinate.HasValue)
            {
                _teleportManager.Teleport(puzzle.PrimaryCoordinate!.Value);
            }

            RouteHandleSolved(puzzleId);

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
                var screenshotStrings = screenshotStatus
                    .Select(x => (CategoryName: ScreenshotManager.GetCategoryName(x.Category), IsRequested: x.IsMissing))
                    .ToList();

                _messageWriter.WriteCetusOpened(screenshotStrings, seenResponse.FirstSighting);
                if (screenshotStrings.Any(x => x.IsRequested))
                {
                    _beeper.BeepForMissingScreenshot();
                }

                _lastOpenedWithSolution = seenResponse.Screenshots.Any(x => x == ScreenshotCategory.Solved) ? puzzleId : default(int?);
            }
            _messageWriter.WriteEndOpened();
            _screenshotManager?.SetLastPuzzle(puzzleId, false);
        }

        private bool ShouldHandlePuzzle(int puzzleId, PuzzleZone zone, PuzzleType type)
        {
            if (WorldInformation.ShouldSkipZone(zone))
            {
                _messageWriter.WriteDebug($"Skipping puzzle in {WorldInformation.GetZoneName(zone)}");
                return false;
            }

            if (WorldInformation.ShouldSkipPuzzleType(type))
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
                        var unsolvedIds = stat.Sightings.Select(s => s.PuzzleId).Except(solved);
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

            return success;
        }

        public ScreenshotManager? GetScreenshotManager()
        {
            return _screenshotManager;
        }

        public async Task OpenCetusWebAsync()
        {
            var code = await _cetusClient.GetSigninCodeAsync();
            try
            {
                _computer.LaunchBrowser($"{_configuration.CetusUri}/account/signin?code={code}");
            }
            catch (Exception e)
            {
                _messageWriter.WriteError($"Failed to launch browser: {e}");
            }
        }

        public void OpenSolutionScreenshot()
        {
            if (_lastOpenedWithSolution == null)
            {
                _messageWriter.WriteError("Last opened puzzle did not have a solved screenshot");
                return;
            }
            try
            {
                _computer.LaunchBrowser($"{_configuration.CetusUri}/goto/solution/{_lastOpenedWithSolution}");
            }
            catch (Exception e)
            {
                _messageWriter.WriteError($"Failed to launch browser: {e}");
            }
        }

        public void OpenPuzzleOnCetus(int puzzleId)
        {
            try
            {
                _computer.LaunchBrowser($"{_configuration.CetusUri}/goto/puzzle/{puzzleId}");
            }
            catch (Exception e)
            {
                _messageWriter.WriteError($"Failed to launch browser: {e}");
            }
        }

        public void Teleport(Coordinate coord)
        {
            _teleportManager.Teleport(coord);
        }

        public void TargetPuzzle(int puzzleId)
        {
            if (!_gamePuzzleHandler.PuzzleDatabase.TryGetValue(puzzleId, out var targetPuzzle))
            {
                _messageWriter.WriteError($"Unknown puzzle id {puzzleId}");
                return;
            }

            var puzzleName = WorldInformation.GetPuzzleName(targetPuzzle.Type);
            var targetCoord = targetPuzzle.PrimaryCoordinate;
            if (targetCoord == null)
            {
                _messageWriter.WriteError($"Puzzle {puzzleId} ({puzzleName}) does not have a coordinate");
                return;
            }

            _teleportManager.SetTarget(targetCoord.Value, targetPuzzle.Type);
            _messageWriter.WriteInfo($"Targeting {puzzleName} with id {puzzleId}");
        }

        public void TargetOtherMatchbox()
        {
            var lastTeleport = _teleportManager.GetLastTeleport();
            if (lastTeleport == null)
            {
                _messageWriter.WriteError("No recorded last teleport");
                return;
            }

            var targetMatchbox = _gamePuzzleHandler.PuzzleDatabase.Values
                .Where(x => x.IsWorldPuzzle && x.Type == PuzzleType.MatchBox)
                .Select(x => new[] { (Puzzle: x, Coordinate: x.PrimaryCoordinate), (Puzzle: x, Coordinate: x.SecondaryCoordinate) })
                .SelectMany(x => x)
                .Where(x => x.Coordinate != null)
                .Select(x => (x.Puzzle, Distance: x.Coordinate!.Value.GetDistance2d(lastTeleport.Value)))
                .OrderBy(x => x.Distance)
                .FirstOrDefault();

            if (targetMatchbox == default)
            {
                _messageWriter.WriteError("No closest matchbox, this is weird (and probably a bug)");
                return;
            }

            var boxes = new[] { targetMatchbox.Puzzle.PrimaryCoordinate.Value, targetMatchbox.Puzzle.SecondaryCoordinate.Value };
            var furthest = boxes.OrderByDescending(x => x.GetDistance2d(lastTeleport.Value)).FirstOrDefault();
            _messageWriter.WriteInfo($"Targeting Matchbox {targetMatchbox.Puzzle.KrakenId}");
            _teleportManager.SetTarget(furthest, PuzzleType.MatchBox);
        }

        public async Task ImportSaveGameAsync()
        {
            var saveFilePath = _computer.GetOfflineSaveFile();
            if (!File.Exists(saveFilePath))
            {
                _messageWriter.WriteError($"Could not find file '{saveFilePath}'");
                return;
            }

            ImportSaveResponse? response;
            await using (var fs = File.OpenRead(saveFilePath))
            {
                response = await _cetusClient.ImportSaveGameAsync(fs);
            }

            if (response == null)
            {
                _messageWriter.WriteError("Something went wrong");
                return;
            }

            var allSolvedIds = response
                .AllSolves
                .OrderBy(x => x)
                .Select(x => x.ToString());

            const string solvedPuzzleIdFile = "solvedpuzzleids.txt";
            _messageWriter.WriteInfo($"Saving {response.AllSolves.Length} solved puzzle ids to " + solvedPuzzleIdFile);
            await File.WriteAllTextAsync(solvedPuzzleIdFile, string.Join(",", allSolvedIds));

            _messageWriter.WriteInfo($"Server flagged {response.ServerAddedCount} previously unsolved puzzles as solved");
            _messageWriter.WriteInfo($"Server flagged {response.ServerRemovedCount} previously solved puzzles as unsolved as they were stale");

            //Update local db
            _db.ImportSaves(response.ImportedSolves, response.AllSolves.ToHashSet(), response.MostRecentSolve);
        }

        public async Task ListClosestAsync(int numberToList, bool use2dDistance)
        {
            var lastTeleport = _teleportManager.GetLastTeleport();
            if (lastTeleport == null)
            {
                _messageWriter.WriteError("No recorded last teleport");
                return;
            }

            double GetDistance(Coordinate a, Coordinate b)
            {
                if (use2dDistance) return a.GetDistance2d(b);
                return a.GetDistance3d(b);
            }

            var worldPuzzles = _gamePuzzleHandler.PuzzleDatabase.Values
                .Where(x => x.IsWorldPuzzle)
                .Select(x => new[] { (Puzzle: x, Coordinate: x.PrimaryCoordinate), (Puzzle: x, Coordinate: x.SecondaryCoordinate) })
                .SelectMany(x => x)
                .Where(x => x.Coordinate != null)
                .Select(x => (x.Puzzle, Distance: GetDistance(x.Coordinate!.Value, lastTeleport.Value)))
                .OrderBy(x => x.Distance)
                .Take(numberToList)
                .ToList();

            var hasCetusStatus = false;
            Dictionary<int, PuzzleStatus> puzzleStatus = new();
            if (IsOnline())
            {
                var puzzleIds = worldPuzzles.Select(x => x.Puzzle.KrakenId)
                    .ToArray();
                var request = new PuzzleStatusRequest() { PuzzleIds = puzzleIds };
                var puzzleStatusResponse = await _cetusClient.GetPuzzleStatusAsync(request);
                hasCetusStatus = puzzleStatusResponse != null;
                if (puzzleStatusResponse != null)
                {
                    puzzleStatus = puzzleStatusResponse.PuzzleStatus;
                }
            }

            var distanceModels = worldPuzzles.Select(x =>
            {
                var cetusStatus = puzzleStatus!.GetValueOrDefault(x.Puzzle.KrakenId, null);
                return new MessageWriter.DistanceModel()
                {
                    PuzzleId = x.Puzzle.KrakenId,
                    PuzzleName = WorldInformation.GetPuzzleName(x.Puzzle.Type),
                    IsSolved = _db.IsSolved(x.Puzzle.KrakenId),
                    Distance = x.Distance,
                    Seen = cetusStatus?.SeenInCurrentCycle,
                    RequestsScreenshot = cetusStatus?.ScreenshotRequested,
                };
            });

            _messageWriter.WriteClosest(distanceModels, hasCetusStatus);
        }

        #region Routing

        private void RouteHandleSolved(int puzzleId)
        {
            if (!_puzzleRouter.HasRoute()) return;

            _puzzleRouter.AddSolved(puzzleId);
            var current = _puzzleRouter.CurrentNode();
            if (current == null) return;
            if (current.Value.Node.Puzzle.KrakenId != puzzleId)
            {
                // Retarget the current node if it's not the one we just solved.
                _teleportManager.SetTarget(current.Value.Node.Puzzle.PrimaryCoordinate!.Value, current.Value.Node.Puzzle.Type);
            }

            NextRouteWaypoint();
        }

        public void GenerateUnsolvedWaypoints(PuzzleZone puzzleZone, PuzzleType puzzleType)
        {
            var pool = _gamePuzzleHandler.PuzzleDatabase.Values
                .Where(x => x.IsWorldPuzzle && x.Zone == puzzleZone && x.Type == puzzleType)
                .ToList();

            var solved = _db.GetSolvedIds(puzzleZone, puzzleType).ToList();
            var unsolvedPool = pool.Where(x => !solved.Contains(x.KrakenId)).ToList();

            var startCoordinate = _teleportManager.GetLastTeleport() ?? default;
            _puzzleRouter.SetRoute(unsolvedPool.Select(x => new RouteNode()
            {
                Puzzle = x,
                Servers = null,
                Stale = null,

            }), startCoordinate);

            NextRouteWaypoint();
        }

        public async Task GenerateUnsolvedRouteFromCetus(PuzzleZone zone)
        {
            ClearRoute();

            var puzzles = await _cetusClient.GetSightedUnsolved(zone);
            if (puzzles == null)
            {
                _messageWriter.WriteError("Failed to get unsolved puzzles");
                return;
            }

            var unsolved = puzzles.Unsolved;
            var pool = _gamePuzzleHandler.PuzzleDatabase.Values
                .Where(x => x.IsWorldPuzzle && x.Zone == zone)
                .ToList();

            var joined = pool.Join(unsolved , x => x.KrakenId, x => x.PuzzleId, (puzzle, unsolved) => (puzzle, unsolved))
                .ToList();
            if (!joined.Any())
            {
                _messageWriter.WriteInfo("No known unsolved puzzles");
                return;
            }

            var startCoordinate = _teleportManager.GetLastTeleport() ?? default;
            _puzzleRouter.SetRoute(joined.Select(x => new RouteNode()
            {
                Puzzle = x.puzzle,
                Servers = x.unsolved.Servers,
                Stale = x.unsolved.IsStale,

            }), startCoordinate);

            NextRouteWaypoint();
        }

        private void WriteWaypointMessage(RouteNode node, int current, int max)
        {
            _messageWriter.WriteWaypointMessage(new MessageWriter.WaypointMessage()
            {
                PuzzleId = node.Puzzle.KrakenId,
                PuzzleName = WorldInformation.GetPuzzleName(node.Puzzle.Type),
                CurrentWaypoint = current,
                TotalWaypoints = max,
                Stale = node.Stale,
                IsOnCurrentServer = node.Servers?.Contains(_serverAddress),
            });
        }

        public void NextRouteWaypoint()
        {
            var next = _puzzleRouter.NextNode();
            var currentCoordinate = _teleportManager.GetLastTeleport() ?? default;

            if (next == null) return;

            _teleportManager.SetTarget(next.Value.Node.Puzzle.PrimaryCoordinate!.Value, next.Value.Node.Puzzle.Type);
            WriteWaypointMessage(next.Value.Node, next.Value.Index, next.Value.Max);
            TeleportManager.WriteDistance(currentCoordinate, next.Value.Node.Puzzle.PrimaryCoordinate!.Value, _messageWriter, null);
        }

        public void CurrentRouteWaypoint()
        {
            var current = _puzzleRouter.CurrentNode();
            if (current == null) return;
        }

        public void OpenCurrentWaypointOnCetus()
        {
            var current = _puzzleRouter.CurrentNode();
            if (current == null)
            {
                _messageWriter.WriteError("No current route");
                return;
            }
            OpenPuzzleOnCetus(current.Value.Node.Puzzle.KrakenId);
        }

        public void PreviousRouteWaypoint()
        {
            var previous = _puzzleRouter.PreviousNode();
            var currentCoordinate = _teleportManager.GetLastTeleport() ?? default;

            if (previous == null) return;
            _teleportManager.SetTarget(previous.Value.Node.Puzzle.PrimaryCoordinate!.Value, previous.Value.Node.Puzzle.Type);
            WriteWaypointMessage(previous.Value.Node, previous.Value.Index, previous.Value.Max);
            TeleportManager.WriteDistance(currentCoordinate, previous.Value.Node.Puzzle.PrimaryCoordinate!.Value, _messageWriter, null);
        }

        public bool HasRoute()
        {
            return _puzzleRouter.HasRoute();
        }

        public void ClearRoute()
        {
            _puzzleRouter.ClearRoute();;
        }
        #endregion
    }
}
