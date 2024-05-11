using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using InsightLogParser.Common.World;

namespace InsightLogParser.Client.Parsing
{
    internal class LogParser
    {
        private readonly Action<string> _logLineCallback;
        private const string TimestampPattern = @"^\[(.{19}):\d{3}\]";
        private static readonly Regex _playerIdRegex = new Regex(TimestampPattern + @".*Player sign-in to Kraken succeeded with user ID (.{36})", RegexOptions.Compiled, TimeSpan.FromSeconds(1));
        private static readonly Regex _prepRegex = new Regex(TimestampPattern + @".*About to record BhvrAnalytics event named \""(.*)\""", RegexOptions.Compiled, TimeSpan.FromSeconds(1));
        private static readonly Regex _eventRegex = new Regex(TimestampPattern + @".*Attribute ""data"" has value \""(.*)\""", RegexOptions.Compiled, TimeSpan.FromSeconds(1));
        private static readonly Regex _restartHandshakeRegex = new Regex(TimestampPattern + @".*Beginning restart handshake process", RegexOptions.Compiled, TimeSpan.FromSeconds(1));
        private static readonly Regex _stopRegex = new Regex(TimestampPattern + @".*Log file closed", RegexOptions.Compiled, TimeSpan.FromSeconds(1));
        private static readonly Regex _teleportRegex = new Regex(TimestampPattern + @".*Teleporting to X=([-0-9.]+) Y=([-0-9.]+) Z=([-0-9.]+)", RegexOptions.Compiled, TimeSpan.FromSeconds(1));
        private static readonly Regex _joinedServer = new Regex(TimestampPattern + @".*UPendingNetGame::SendInitialJoin.*RemoteAddr: ([0-9\.:]+), Name", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

        public LogParser(Action<string> logLineCallback)
        {
            _logLineCallback = logLineCallback;
        }

        public async IAsyncEnumerable<LogEvent> LogEvents(LogReader reader, [EnumeratorCancellation] CancellationToken token = default)
        {
            var playerId = Guid.Empty;
            string? lastEvent = null;
            await foreach (var line in reader.ReadLinesAsync(token).ConfigureAwait(false))
            {
                _logLineCallback(line);

                var prep = MatchEventPrep(line);
                if (prep != null)
                {
                    lastEvent = prep.Value.eventName;
                    continue;
                }

                var @event = MatchEvent(line, lastEvent);
                if (@event != null)
                {
                    yield return new LogEvent
                    {
                        Type = LogEventType.PuzzleEvent,
                        LogTime = @event.Value.timestamp,
                        PlayerId = playerId,
                        Event = @event.Value.parsedEvent,
                    };
                    continue;
                }

                var si = MatchSessionInfo(line);
                if (si != null)
                {
                    playerId = si.Value.playerId;
                    yield return new LogEvent
                    {
                        Type = LogEventType.PlayerIdentified,
                        LogTime = si.Value.sessionStart,
                        PlayerId = playerId,
                    };
                    continue;
                }

                var restart = MatchRestartHandshake(line);
                if (restart != null)
                {
                    yield return new LogEvent
                    {
                        Type = LogEventType.SessionRestartHandshake,
                        LogTime = restart.Value,
                        PlayerId = playerId,
                    };
                    continue;
                }

                var teleport = MatchTeleport(line);
                if (teleport != null)
                {
                    yield return new LogEvent
                    {
                        Type = LogEventType.Teleport,
                        Event = null,
                        PlayerId = playerId,
                        LogTime = teleport.Value.eventTime,
                        Coordinate = new Coordinate(teleport.Value.x, teleport.Value.y, teleport.Value.z),
                    };
                }

                var joinedServer = MatchJoinedServer(line);
                if (joinedServer != null)
                {
                    yield return new LogEvent()
                    {
                        Type = LogEventType.JoinedServer,
                        ServerAddress = joinedServer.Value.serverAddress
                    };
                }


                var end = MatchEnd(line);
                if (end != null)
                {
                    yield return new LogEvent
                    {
                        Type = LogEventType.SessionEnd,
                        LogTime = end.Value,
                        PlayerId = playerId,
                    };
                    yield break; //This will be the last line in the log so no need to proceed
                }
            }
        }

        private DateTimeOffset? MatchEnd(string line)
        {
            var result = _stopRegex.Match(line);
            if (!result.Success) return null;

            var timestamp = result.Groups[1].Value;
            var (stampSuccess, eventTime) = ParseLogDate(timestamp);
            return eventTime;
        }

        private DateTimeOffset? MatchRestartHandshake(string line)
        {
            var result = _restartHandshakeRegex.Match(line);
            if (!result.Success) return null;

            var timestamp = result.Groups[1].Value;
            var (stampSuccess, eventTime) = ParseLogDate(timestamp);
            return eventTime;
        }

        private (DateTimeOffset timestamp, ParsedLogEvent parsedEvent)? MatchEvent(string line, string? lastEvent)
        {
            var result = _eventRegex.Match(line);
            if (!result.Success) return null;

            var timestamp = result.Groups[1].Value;
            var (stampSuccess, eventTime) = ParseLogDate(timestamp);

            var json = result.Groups[2].Value;
            var parsedEvent = JsonSerializer.Deserialize<ParsedLogEvent>(json);
            if (parsedEvent == null) return null;
            if (string.IsNullOrWhiteSpace(parsedEvent.EventType) && lastEvent != null)
            {
                parsedEvent.EventType = lastEvent;
            }

            return (eventTime, parsedEvent);
        }

        private (DateTimeOffset timestamp, string eventName)? MatchEventPrep(string line)
        {
            var result = _prepRegex.Match(line);
            if (!result.Success) return null;

            var timestamp = result.Groups[1].Value;
            var (stampSuccess, eventTime) = ParseLogDate(timestamp);

            var eventName = result.Groups[2].Value;
            if (stampSuccess)
            {
                return (eventTime, eventName);
            }
            return null;
        }

        private (DateTimeOffset sessionStart, Guid playerId)? MatchSessionInfo(string line)
        {
            var result = _playerIdRegex.Match(line);
            if (!result.Success) return null;

            var timestamp = result.Groups[1].Value;
            var (stampSuccess, eventTime) = ParseLogDate(timestamp);

            var guidString = result.Groups[2].Value;
            var guidSuccess = Guid.TryParse(guidString, out var playerGuid);
            if (stampSuccess && guidSuccess)
            {
                return (eventTime, playerGuid);
            }
            return null;
        }

        private (DateTimeOffset eventTime, float x, float y, float z)? MatchTeleport(string line)
        {
            var result = _teleportRegex.Match(line);
            if (!result.Success) return null;

            var timestamp = result.Groups[1].Value;
            var (_, eventTime) = ParseLogDate(timestamp);
            if (!float.TryParse(result.Groups[2].Value, CultureInfo.InvariantCulture, out var x)) return null;
            if (!float.TryParse(result.Groups[3].Value, CultureInfo.InvariantCulture, out var y)) return null;
            if (!float.TryParse(result.Groups[4].Value, CultureInfo.InvariantCulture, out var z)) return null;
            return (eventTime, x, y, z);
        }

        private (DateTimeOffset eventTime, string serverAddress)? MatchJoinedServer(string line)
        {
            var result = _joinedServer.Match(line);
            if (!result.Success) return null;
            var timestamp = result.Groups[1].Value;
            var (_, eventTime) = ParseLogDate(timestamp);
            return (eventTime, result.Groups[2].Value);
        }


        private (bool success, DateTimeOffset result) ParseLogDate(string timestamp)
        {
            var success = DateTimeOffset.TryParseExact(timestamp, "yyyy.MM.dd-HH.mm.ss", CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal, out var eventTime);
            return (success, eventTime);
        }
    }
}
