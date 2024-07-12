using System.Globalization;
using System.Text;
using System.Text.Json;
using InsightLogParser.Common.ApiModels;
using InsightLogParser.Common.ParsedDbModels;
using InsightLogParser.Common.PuzzleParser;
using InsightLogParser.Common.World;

namespace InsightLogParser.Client
{
    internal class ParsedDatabase
    {
        private readonly string _jsonPath;
        private readonly MessageWriter _messageWriter;
        private readonly string _tempPath;
        private ParsedDb _db = null!;

        //Running states
        private bool _started = false;
        private readonly CancellationTokenSource _cts;
        private Task _periodicSaveTask = null!;

        private readonly object _lock = new();
        private bool _dirty;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ParsedDatabase(string jsonPath, MessageWriter messageWriter, CancellationToken forcedExitToken)
        {
            _jsonPath = jsonPath;
            _messageWriter = messageWriter;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(forcedExitToken);
            var pathPart = Path.GetDirectoryName(_jsonPath);
            if (string.IsNullOrWhiteSpace(pathPart)) pathPart = ".";
            var filePart = Path.GetFileNameWithoutExtension(_jsonPath);
            var extensionPart = Path.GetExtension(_jsonPath);
            var tempName = $"{filePart}-temp{extensionPart}";
            _tempPath = Path.Combine(pathPart, tempName);

            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                WriteIndented = true,
            };
        }

        private ParsedDb GetEmptyDb()
        {
            var zones = Enum.GetValues<PuzzleZone>().Where(x => x != PuzzleZone.Unknown);
            return new ParsedDb
            {
                Version = 1,
                Zones = zones.Select(x => new ZoneNode()
                {
                    Zone = x,
                    SolvedEntries = new Dictionary<PuzzleType, Dictionary<int, PuzzleNode>>(),
                }).ToDictionary(x => x.Zone),
            };
        }

        public async Task StartAsync()
        {
            if (!File.Exists(_jsonPath))
            {
                _db = GetEmptyDb();
            }
            else
            {
                var jsonContent = await File.ReadAllTextAsync(_jsonPath, Encoding.UTF8).ConfigureAwait(ConfigureAwaitOptions.None);
                _db = JsonSerializer.Deserialize<ParsedDb>(jsonContent) ?? GetEmptyDb();
            }

            _periodicSaveTask = StartSaveTimer(_cts.Token);
            var totalSolved = GetTotalSolved();
            _messageWriter.WriteInitLine($"Parsed database loaded with {totalSolved} solved puzzles", ConsoleColor.Green);

            _started = true;
        }

        public async Task StopAsync()
        {
            if (!_started) return;
            _messageWriter.WriteDebug("Stopping parsed database...");
            await _cts.CancelAsync().ConfigureAwait(false);
            await _periodicSaveTask.ConfigureAwait(false);
            await SaveDatabaseAsync().ConfigureAwait(false);
            _messageWriter.WriteDebug("Parsed database stopped");
        }

        private async Task SaveDatabaseAsync()
        {
            string json;
            lock (_lock)
            {
                if (!_dirty)
                {
                    _messageWriter.WriteDebug("Parsed database is up to date");
                    return;
                }

                json = JsonSerializer.Serialize(_db, _jsonSerializerOptions);
                _dirty = false;
            }

            await File.WriteAllTextAsync(_tempPath, json, Encoding.UTF8).ConfigureAwait(ConfigureAwaitOptions.None);
            File.Delete(_jsonPath);
            File.Move(_tempPath, _jsonPath);
            _messageWriter.WriteInfo("Saved parsed database");
        }

        private async Task StartSaveTimer(CancellationToken cancelToken)
        {
            using (var timer = new PeriodicTimer(TimeSpan.FromSeconds(60)))
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        await timer.WaitForNextTickAsync(cancelToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        _messageWriter.WriteDebug("DB save timer cancelled");
                    }
                    await SaveDatabaseAsync().ConfigureAwait(ConfigureAwaitOptions.None);
                }
            }
        }

        public bool AddSolved(PuzzleZone zone, PuzzleType type, int id, DateTimeOffset solvedTime)
        {
            if (_started == false) throw new InvalidOperationException("Not started yet");

            lock (_lock)
            {
                if (!_db.Zones.TryGetValue(zone, out var zoneInfo))
                {
                    return false;
                }

                if (!zoneInfo.SolvedEntries.ContainsKey(type))
                {
                    zoneInfo.SolvedEntries.Add(type, new ());
                }
                var entries = zoneInfo.SolvedEntries[type];
                if (!entries.ContainsKey(id))
                {
                    //Puzzle has never been parsed as solved
                    entries.Add(id, new()
                    {
                        Solves = new List<DateTimeOffset>()
                        {
                            solvedTime,
                        }
                    });
                    _dirty = true;
                    return true;
                }

                //Puzzle has previously been parsed as solved
                var entry = entries[id];
                if (!entry.Solves.Contains(solvedTime))
                {
                    entry.Solves.Add(solvedTime);
                    _dirty = true;
                    return true;
                }

                return false;
            }
        }

        public int GetTotalSolved()
        {
            lock (_lock)
            {
                return _db.Zones.Values
                    .SelectMany(x => x.SolvedEntries)
                    .SelectMany(x => x.Value.Keys)
                    .Distinct()
                    .Count();
            }
        }

        public IReadOnlyDictionary<PuzzleZone, IReadOnlyDictionary<PuzzleType, IReadOnlyList<int>>> GetSolvedIdsPerZoneAndType()
        {
            lock (_lock)
            {
                return _db.Zones.ToDictionary(x => x.Key
                    , x => (IReadOnlyDictionary<PuzzleType, IReadOnlyList<int>>)x.Value.SolvedEntries.ToDictionary(x2 => x2.Key
                        , x2 => (IReadOnlyList<int>)x2.Value.Keys.ToList()));
            }
        }

        public IEnumerable<int> GetAllSolvedSince(PuzzleZone zone, DateTimeOffset timeframe)
        {
            var allSolved = _db.Zones[zone].SolvedEntries.SelectMany(x => x.Value)
                .Where(x => x.Value.Solves.Max() >= timeframe)
                .Select(x => x.Key);
            return allSolved;
        }

        public IEnumerable<int> GetAllSolvedSince(PuzzleZone zone, PuzzleType type, DateTimeOffset timeframe)
        {
            var allSolved = _db.Zones[zone].SolvedEntries[type]
                .Where(x => x.Value.Solves.Max() >= timeframe)
                .Select(x => x.Key);
            return allSolved;
        }

        public (PuzzleZone Zone, PuzzleType PuzzleType, int PuzzleId, DateTimeOffset LastSolved)? GetByPuzzleId(int otherId)
        {
            if (_started == false) throw new InvalidOperationException("Not started yet");
            lock (_lock)
            {
                var allZones = _db.Zones.Select(x => (x.Key, x.Value))
                    .SelectMany(x => x.Value.SolvedEntries, (a, b) => (Zone: a.Key, PuzzleType: b.Key, Solved: b.Value))
                    .SelectMany(x => x.Solved, (a, b) => (a.Zone, a.PuzzleType, PuzzleId: b.Key, LastSolved: b.Value.Solves.Max()))
                    .GroupBy(x => x.PuzzleId)
                    .ToDictionary(x => x.Key, x => x.MaxBy(y => y.LastSolved));
                if (!allZones.TryGetValue(otherId, out var result)) return null;
                return result;
            }
        }

        public List<(int Id, DateTimeOffset LastSolved)>? GetSolves(PuzzleZone zone, PuzzleType type)
        {
            if (!_started) throw new InvalidOperationException("Not started yet");
            lock (_lock)
            {
                if (!_db.Zones.TryGetValue(zone, out var puzzleZone)) return null;
                if (!puzzleZone.SolvedEntries.TryGetValue(type, out var puzzleType)) return null;
                return puzzleType.Select(x => (Id: x.Key, LastSolved: x.Value.Solves.OrderByDescending(o => o).FirstOrDefault()))
                    .ToList();
            }
        }

        public IEnumerable<int> GetSolvedIds(PuzzleZone zone, PuzzleType type)
        {
            if (!_started) throw new InvalidOperationException("Not started yet");
            lock (_lock)
            {
                if (!_db.Zones.TryGetValue(zone, out var puzzleZone)) return Enumerable.Empty<int>();
                if (!puzzleZone.SolvedEntries.TryGetValue(type, out var puzzleType)) return Enumerable.Empty<int>();
                return puzzleType.Keys;
            }
        }

        public DateTimeOffset? GetLastSolved(PuzzleZone zone, PuzzleType type, int id)
        {
            if (!_started) throw new InvalidOperationException("Not started yet");
            lock (_lock)
            {
                if (!_db.Zones.TryGetValue(zone, out var puzzleZone)) return null;
                if (!puzzleZone.SolvedEntries.TryGetValue(type, out var puzzleType)) return null;
                if (!puzzleType.TryGetValue(id, out var puzzle)) return null;

                return puzzle.Solves
                    .OrderByDescending(x => x)
                    .FirstOrDefault();
            }
        }

        public (int? parsedCount, int? cycleCount, int? totalCount) GetParsedCount(PuzzleZone zone, PuzzleType type, int id, DateTimeOffset since)
        {
            if (!_started) throw new InvalidOperationException("Not started yet");
            lock (_lock)
            {
                if (!_db.Zones.TryGetValue(zone, out var puzzleZone)) return default;
                if (!puzzleZone.SolvedEntries.TryGetValue(type, out var puzzleType)) return default;

                var count = puzzleType[id].Solves.Count;

                var cycleCount = puzzleType
                    .Where(x => x.Value.Solves.Any(s => s > since))
                    .Select(x => x.Key)
                    .Distinct()
                    .Count();

                return (count, cycleCount, puzzleType.Keys.Count);
            }
        }

        public void RemoveNonWorldPuzzles(GamePuzzleHandler puzzleHandler)
        {
            lock (_lock)
            {
                var worldPuzzleIds = puzzleHandler.PuzzleDatabase.Values
                    .Where(x => x.IsWorldPuzzle)
                    .Select(x => x.KrakenId)
                    .ToHashSet();
                foreach (var zoneKey in _db.Zones.Keys)
                {
                    var zone = _db.Zones[zoneKey];
                    foreach (var type in zone.SolvedEntries.Keys)
                    {
                        var solvedPuzzles = zone.SolvedEntries[type];
                        var valid = solvedPuzzles.Keys.Intersect(worldPuzzleIds);
                        var toRemove = solvedPuzzles.Keys.Except(valid).ToList();
                        var removeCount = toRemove.Count;
                        if (removeCount > 0)
                        {
                            foreach (var puzzleId in toRemove)
                            {
                                solvedPuzzles.Remove(puzzleId);
                            }
                            _dirty = true;
                            var puzzleName = WorldInformation.GetPuzzleName(type);
                            var zoneName = WorldInformation.GetZoneName(zoneKey);

                            _messageWriter.WriteInitLine($"Cleaning up {removeCount} non-world {puzzleName} in {zoneName}", ConsoleColor.Yellow);
                        }
                    }
                }

                if (_dirty)
                {
                    MakeBackup();
                }
            }
        }

        public void ImportSaves(ImportSolve[] importedSolves, HashSet<int> allSolvedIds, DateTimeOffset mostRecentSolveTime)
        {
            lock (_lock)
            {
                foreach (var puzzleZone in _db.Zones.Keys)
                {
                    var zoneNode = _db.Zones[puzzleZone];
                    foreach (var type in zoneNode.SolvedEntries.Keys)
                    {
                        var localSolves = zoneNode.SolvedEntries[type];

                        //Add new solves
                        var imported = importedSolves
                            .Where(x => x.PuzzleZone == puzzleZone && x.PuzzleType == type)
                            .ToList();
                        var importedCounter = 0;
                        foreach (var importSolve in imported)
                        {
                            if (!localSolves.ContainsKey(importSolve.PuzzleId))
                            {
                                localSolves.Add(importSolve.PuzzleId, new PuzzleNode
                                {
                                    Solves = [importSolve.SolvedAt]
                                });
                                _dirty = true;
                                importedCounter++;
                            }
                        }

                        //Remove stale solves
                        var stales = new List<(int puzzleId, PuzzleNode node)>();
                        foreach (var (puzzleId, node) in localSolves)
                        {
                            //If it is solved, it is not stale
                            if (allSolvedIds.Contains(puzzleId)) continue;

                            //If we have a solve after the most recent time, we can't say if it's stale or not
                            if (node.Solves.Any(x => x >= mostRecentSolveTime)) continue;

                            stales.Add((puzzleId, node));
                        }

                        foreach (var valueTuple in stales)
                        {
                            localSolves.Remove(valueTuple.puzzleId);
                            _dirty = true;
                        }
                        var puzzleName = WorldInformation.GetPuzzleName(type);
                        var zoneName = WorldInformation.GetZoneName(puzzleZone);

                        if (importedCounter > 0) _messageWriter.WriteInfo($"Imported {importedCounter} solves of {puzzleName} from {zoneName} to local db", ConsoleColor.Green);
                        if (stales.Count > 0) _messageWriter.WriteInfo($"Removed {stales.Count} stale local solves of {puzzleName} from {zoneName}", ConsoleColor.Yellow);
                    }
                }

                if (_dirty)
                {
                    var backupPath = MakeBackup();
                    _messageWriter.WriteInfo($"Changes will be made to the local db, a backup has been saved at {backupPath}", ConsoleColor.Yellow);
                }
                else
                {
                    _messageWriter.WriteInfo("Local db was already up to date");
                }
            }
        }

        private string MakeBackup()
        {
            var pathPart = Path.GetDirectoryName(_jsonPath);
            if (string.IsNullOrWhiteSpace(pathPart)) pathPart = ".";
            var filePart = Path.GetFileNameWithoutExtension(_jsonPath);
            var extensionPart = Path.GetExtension(_jsonPath);
            var timestring = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            var backupName = $"{filePart}-backup-{timestring}{extensionPart}";
            File.Copy(_jsonPath, backupName);
            return backupName;
        }

        public bool IsSolved(int puzzleKrakenId)
        {
            lock (_lock)
            {
                foreach (var zoneNode in _db.Zones.Values)
                {
                    if (zoneNode.SolvedEntries.Values.Any(x => x.ContainsKey(puzzleKrakenId))) return true;
                }

                return false;
            }
        }
    }
}
