using InsightLogParser.Common.World;

namespace InsightLogParser.Client.Parsing
{
    internal class LogProcessor
    {
        private readonly MessageWriter _messageWriter;
        private readonly UserComputer _computer;
        private readonly Spider _spider;
        private readonly TeleportManager _teleportManager;
        private readonly CancellationTokenSource _stopTokenSource;

        private Task _processingTask = null!;
        private bool _started;

        public LogProcessor(MessageWriter messageWriter
            , UserComputer computer
            , CancellationToken forcedExitToken
            , Spider spider
            , TeleportManager teleportManager
        )
        {
            _messageWriter = messageWriter;
            _computer = computer;
            _spider = spider;
            _teleportManager = teleportManager;
            _stopTokenSource = CancellationTokenSource.CreateLinkedTokenSource(forcedExitToken);
        }

        public async Task StartAsync(string logfilePath)
        {
            _started = true;
            await Task.Yield();
            _messageWriter.WriteDebug("Starting log processor");
            _processingTask = ProcessParse(logfilePath);
        }

        public async Task StopAsync()
        {
            if (!_started) return;
            _messageWriter.WriteDebug("Stopping log processor");
            await _stopTokenSource.CancelAsync().ConfigureAwait(ConfigureAwaitOptions.None);
            await _processingTask.ConfigureAwait(ConfigureAwaitOptions.None);
        }

        private async Task ProcessParse(string logfilePath)
        {
            while (!_stopTokenSource.IsCancellationRequested)
            {
                //If the game is not running, start monitoring the log file for changes
                if (!_computer.IsGameRunning())
                {
                    _messageWriter.WriteInfo("Waiting for game to start logging");
                    _messageWriter.WriteDebug("Game is not running, setting up FileSystemWatcher");
                    //Game is not running, start monitoring for the log file
                    var logFolder = Path.GetDirectoryName(logfilePath);
                    if (string.IsNullOrWhiteSpace(logFolder)) logFolder = ".";
                    var logFileName = Path.GetFileName(logfilePath);
                    var tcs = new TaskCompletionSource();
                    await using (_stopTokenSource.Token.Register(() =>
                                 {
                                     _messageWriter.WriteDebug("Cancelling the task waiting for FileSystemWatcher");
                                     tcs.TrySetCanceled();
                                 }))
                    {
                        var fsw = new FileSystemWatcher(logFolder, logFileName)
                        {
                            EnableRaisingEvents = true,
                            IncludeSubdirectories = false,
                        };
                        void FswOnChanged(object sender, FileSystemEventArgs args)
                        {
                            if (args.Name != logFileName) return;
                            _messageWriter.WriteDebug("Detected log file being created/modified");
                            tcs.TrySetResult();
                        }

                        fsw.Created += FswOnChanged;
                        fsw.Changed += FswOnChanged;

                        using (fsw)
                        {
                            //Check if game has started while setting up the watcher
                            if (!_computer.IsGameRunning())
                            {
                                _messageWriter.WriteDebug("Waiting for log file to be created");
                                await tcs.Task.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                                if (_stopTokenSource.IsCancellationRequested)
                                {
                                    //We're stopping, so we should not attempt to start parsing
                                    break;
                                }
                                _messageWriter.WriteDebug("Done waiting for log file");
                                //Wait a second before starting the parse
                                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                            }
                        }
                    }
                }

                //Game is running now (or something else is tinkering with the log file)
                var parser = new LogParser(_spider.WriteRawLogLine, _messageWriter);
                using (var reader = new LogReader(logfilePath))
                {
                    await foreach (var logEvent in parser.LogEvents(reader, _stopTokenSource.Token).ConfigureAwait(false))
                    {
                        switch (logEvent.Type)
                        {
                            case LogEventType.ConnectingToServer:
                                _spider.ConnectingToServer(logEvent.ServerAddress!);
                                break;

                            case LogEventType.JoinedServer:
                                _spider.SetServer(logEvent.ServerAddress!, logEvent.LogTime);
                                break;

                            case LogEventType.SessionEnd:
                                _spider.EndSession(logEvent.LogTime);
                                break;

                            case LogEventType.SessionRestartHandshake:
                                await _spider.SessionBeingDisconnectedAsync();
                                break;

                            case LogEventType.PuzzleEvent:
                                if (logEvent.Event == null)
                                {
                                    _messageWriter.WriteDebug("logEvent.Event was null, this is probably a bug");
                                    break;
                                }

                                var zone = WorldInformation.GetPuzzleZone(logEvent.Event.PuzzleArea);
                                var type = WorldInformation.GetPuzzleTypeByLogName(logEvent.Event.PuzzleName);
                                var difficulty = logEvent.Event.Difficulty > 0 ? logEvent.Event.Difficulty : default(short?);
                                var puzzleId = logEvent.Event.PuzzleId;
                                switch (logEvent.Event.EventType)
                                {
                                    case "gm_solve_puzzle":
                                        await _spider.SolvedAsync(logEvent.LogTime, puzzleId, zone, type, difficulty).ConfigureAwait(false);
                                        break;
                                    case "gm_open_puzzle":
                                        await _spider.OpenedAsync(logEvent.LogTime, puzzleId, zone, type, difficulty).ConfigureAwait(false);
                                        break;
                                    case "gm_abandon_puzzle":
                                        await _spider.ClosedAsync(logEvent.LogTime, puzzleId, zone, type, difficulty).ConfigureAwait(false);
                                        break;
                                    case "PressStartButton":
                                        break;
                                    //Ignore so hard it's not even in the debug output
                                    case "gm_heartbeat":
                                    case "gm_base":
                                        break;
                                    default:
                                        _messageWriter.WriteDebug($"Skipping puzzle event of type {logEvent.Event.EventType}");
                                        break;
                                }
                                break;

                            case LogEventType.Teleport:
                                if (logEvent.Coordinate != null)
                                {
                                    _teleportManager.Teleport(logEvent.Coordinate.Value);
                                }
                                else
                                {
                                    _messageWriter.WriteDebug("No coordinates for teleport");
                                }
                                break;

                            default:
                                _messageWriter.WriteDebug($"No handler for event of type {logEvent.Type}");
                                break;
                        }
                    }

                    _messageWriter.WriteDebug("Log file closed or parser closing");
                    //Chill for 10 seconds while the game does it's thing
                    await Task.Delay(TimeSpan.FromSeconds(10), _stopTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                }
            }
            _messageWriter.WriteDebug("Log processor shutting down");
        }
    }
}
