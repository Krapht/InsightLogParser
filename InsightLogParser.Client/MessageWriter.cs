using System.Linq;
using InsightLogParser.Common;

namespace InsightLogParser.Client;

internal class MessageWriter
{
    internal class Statistics
    {
        public string PuzzleName { get; set; } = string.Empty;
        public int CycleSolved { get; set; }
        public int TotalSolved { get; set; }
        public int Total { get; set; }
        public string TimeToCycle { get; set; } = string.Empty;
        public CetusStatistics? CetusStats { get; set; }
    }

    internal class CetusStatistics
    {
        public int Unsolved { get; set; }
        public int Stale { get; set; }
        public int Total { get; set; }
        public int? Expected { get; set; }
    }

    internal class DistanceModel
    {
        public required double Distance { get; set; }
        public required int PuzzleId { get; set; }
        public required string PuzzleName { get; set; }
        public required bool IsSolved { get; set; }

        public bool? Seen { get; set; }
        public bool? RequestsScreenshot { get; set; }
    }

    private readonly object _lock = new();
    internal Func<bool> ShouldShowDebug { get; set; } = () => false;

    public void WriteInitLine(string message, ConsoleColor color)
    {
        lock (_lock)
        {
            Console.ForegroundColor = color;
            Console.Write("[INIT]");
            Console.ResetColor();
            Console.WriteLine($": {message}");
        }
    }

    public void WriteStopMessage(string message, ConsoleColor color)
    {
        lock (_lock)
        {
            Console.ForegroundColor = color;
            Console.Write("[STOP]");
            Console.ResetColor();
            Console.WriteLine($": {message}");
        }
    }

    public void WriteLogLine(string logLine)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(logLine);
            Console.ResetColor();
        }
    }

    public void WriteInfo(string infoLine)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("[INFO]");
            Console.ResetColor();
            Console.WriteLine($": {infoLine}");
        }
    }

    public void WriteDebug(string debugMessage)
    {
        if (!ShouldShowDebug()) return;
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("[DEBUG]");
            Console.ResetColor();
            Console.WriteLine($": {debugMessage}");
        }
    }

    public void WriteError(string errorMessage)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("[ERROR]");
            Console.ResetColor();
            Console.WriteLine($": {errorMessage}");
        }
    }

    public void WriteWelcome()
    {
        lock (_lock)
        {
            Console.WriteLine();
            Console.WriteLine("Welcome to a Insight Log Parser");
            Console.WriteLine();
        }
    }

    public void SessionStarted(DateTimeOffset timestamp)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Session started at {timestamp.ToIso8601()}");
            Console.ResetColor();
        }
    }

    public void ConnectedToServer(string serverAddress)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Game connected to server {serverAddress}");
            Console.ResetColor();
        }
    }

    public void SessionEnded(DateTimeOffset timestamp)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Session ended at {timestamp.ToIso8601()}");
            Console.ResetColor();
        }
    }


    public void SessionDisconnecting()
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("******************************");
            Console.WriteLine("*** DISCONNECTION IMMINENT ***");
            Console.WriteLine("******************************");
            Console.ResetColor();
        }
    }

    public void WriteMenu(IEnumerable<(char? key, string text)> menuOptions)
    {
        lock (_lock)
        {
            foreach (var line in menuOptions)
            {
                if (line.key != null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"[{line.key}] ");
                    Console.ResetColor();
                }
                Console.WriteLine(line.text);
            }
        }
    }

    public void WriteLine(string line)
    {
        lock (_lock)
        {
            Console.WriteLine(line);
        }
    }

    public void WriteParsed(string puzzleZone, string puzzleName, int puzzleId, int? parsedCount, int? cycleCount, int? totalParsed, int total, TimeSpan timeLeftInCycle, TimeSpan? lastSolved)
    {
        lock (_lock)
        {
            Console.Write("Parsed ");
            WritePuzzleName(puzzleName);
            Console.Write($" ({puzzleId}) in ");
            WriteZoneName(puzzleZone);
            Console.Write(". Solved: ");

            if (parsedCount == 1)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("1st parsed solve");
                Console.ResetColor();
            }
            else
            {
                Console.Write($"{parsedCount?.ToString() ?? "??"} times.");
                if (lastSolved.HasValue)
                {
                    Console.Write($" Last time was {lastSolved.Value.ToHHMMSS(false)} ago");
                }
            }
            Console.WriteLine();
            Console.Write($"Cycle: {cycleCount?.ToString() ?? "??"}");
            Console.Write($", Parsed: {totalParsed?.ToString() ?? "??"} / {total}");
            Console.Write($", next cycle in {timeLeftInCycle.ToHHMMSS(true)}");
            Console.WriteLine();
        }
    }

    public void WriteCetusParsed(int sightingsCount, int unsolvedFresh, int unsolvedStale, IReadOnlyList<(string CategoryName, bool IsRequested)>? screenshotCategories)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("[Cetus]: ");
            Console.ResetColor();
            Console.Write($"Unsolved: {unsolvedFresh}, Stale: {unsolvedStale}, TotalSeen: {sightingsCount}");
            if (screenshotCategories != null)
            {
                Console.Write(", Has Screenshot: ");
                foreach (var sc in screenshotCategories)
                {
                    Console.ForegroundColor = sc.IsRequested ? ConsoleColor.Red : ConsoleColor.Green;
                    Console.Write($"{sc.CategoryName} ");
                }
            }
            Console.ResetColor();
        }
    }

    private void WritePuzzleName(string puzzleName)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write(puzzleName);
        Console.ResetColor();
    }

    private void WriteZoneName(string zoneName)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write(zoneName);
        Console.ResetColor();
    }

    public void WriteEndSolved()
    {
        lock (_lock)
        {
            Console.WriteLine();
        }
    }

    public void WriteOpened(string puzzleZone, string puzzleName, int puzzleId, TimeSpan? lastSolved)
    {
        lock (_lock)
        {
            Console.Write("Opened ");
            WritePuzzleName(puzzleName);
            Console.Write($" ({puzzleId}) in ");
            WriteZoneName(puzzleZone);
            Console.WriteLine();

            if (lastSolved == null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("No record of solved");
                Console.ResetColor();
            }
            else
            {
                var ago = lastSolved.Value;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Last solved {ago.Days} days {ago.Hours} hours and {ago.Minutes} minutes ago");
                Console.ResetColor();
            }
        }
    }

    public void WriteCetusOpened(List<(string CategoryName, bool IsRequested)> needsScreenshot)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("[Cetus]: ");
            Console.ResetColor();
            Console.Write("Has Screenshots: ");
            foreach (var sc in needsScreenshot)
            {
                Console.ForegroundColor = sc.IsRequested ? ConsoleColor.Red : ConsoleColor.Green;
                Console.Write($"{sc.CategoryName} ");
            }
            Console.ResetColor();
        }
    }

    public void WriteEndOpened()
    {
        lock (_lock)
        {
            Console.WriteLine();
        }
    }

    public void WriteCycled(string zoneName, string puzzleName)
    {
        lock (_lock)
        {
            WritePuzzleName(puzzleName);
            Console.Write(" just reset in ");
            WriteZoneName(zoneName);
            Console.WriteLine();
        }
    }

    public void WriteNextCycle(string zoneName, string puzzleName, TimeSpan timeToCycle)
    {
        lock (_lock)
        {
            Console.Write("Next cycle will be ");
            WritePuzzleName(puzzleName);
            Console.Write(" in ");
            WriteZoneName(zoneName);
            Console.WriteLine($" in {timeToCycle.ToHHMMSS(true)}");
        }
    }

    public void WriteStatistics(string zoneName, List<Statistics> puzzleStatistics, bool skipCetus)
    {
        var maxNameLength = puzzleStatistics.Select(x => x.PuzzleName.Length).Max();
        lock (_lock)
        {
            Console.WriteLine();
            Console.Write("Puzzle statistics for ");
            WriteZoneName(zoneName);
            Console.WriteLine();
            if (!skipCetus)
            {
                var cetusStart =  maxNameLength + 3 + 5 + 3 + 14 + 2;
                var cetusEnd = 16 + 15 + 3 + 3;

                Console.Write("|".PadLeft(cetusStart));
                Console.WriteLine("Statistics from Cetus |".PadLeft(cetusEnd));
            }
            Console.Write("Puzzle".PadLeft(maxNameLength));
            Console.Write(" | ");
            Console.Write("Cycle");
            Console.Write(" | ");
            Console.Write("Solved / Total");
            Console.Write(" | ");
            if (!skipCetus)
            {
                Console.Write("Unparsed / Stale");
                Console.Write(" | ");
                Console.Write("Seen / Expected");
                Console.Write(" | ");
            }
            Console.Write("Reset in (hh:mm:ss)");
            Console.WriteLine();
            foreach (var stat in puzzleStatistics)
            {
                var paddedName = stat.PuzzleName.PadLeft(maxNameLength);
                WritePuzzleName(paddedName);
                Console.Write(" | ");
                Console.Write($"{stat.CycleSolved,5}");
                Console.Write(" | ");
                Console.Write($"{stat.TotalSolved,3} / {stat.Total,3}".PadLeft(14));
                Console.Write(" | ");
                if (!skipCetus)
                {
                    var cetus = stat.CetusStats;
                    Console.Write($"{cetus?.Unsolved, 3} / {cetus?.Stale,3}".PadLeft(16));
                    Console.Write(" | ");
                    var expectedString = cetus?.Expected?.ToString() ?? "??";
                    Console.Write($"{cetus?.Total, 3} / {expectedString, 3}".PadLeft(15));
                    Console.Write(" | ");
                }
                Console.Write(stat.TimeToCycle);
                Console.WriteLine();
            }
        }
    }

    public void WriteSightings(string zoneName, string puzzleName, IEnumerable<(int PuzzleId, bool Stale, List<string> Servers)> entries, string currentServer)
    {
        lock (_lock)
        {
            Console.Write("Sightings from Cetus for ");
            WritePuzzleName(puzzleName);
            Console.Write(" in ");
            WriteZoneName(zoneName);
            Console.WriteLine();

            Console.Write(" #");
            Console.Write(" | ");
            Console.Write("Puzzle Id");
            Console.Write(" | ");
            Console.Write("Stale");
            Console.Write(" | ");
            Console.WriteLine("Server address");

            var i = 1;
            foreach (var entry in entries)
            {
                Console.Write($"{i++, 2}");
                Console.Write(" | ");
                Console.Write($"{entry.PuzzleId, 5}".PadLeft(9));
                Console.Write(" | ");
                if (entry.Stale)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Yes".PadLeft(5));
                    Console.ResetColor();
                }
                else
                {
                    Console.Write("No".PadLeft(5));
                }
                Console.Write(" | ");
                var firstServer = true;
                foreach (var server in entry.Servers)
                {
                    if (!firstServer)
                    {
                        Console.Write(" | ".PadLeft(25));
                    }
                    if (server == currentServer)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(server);
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine(server);
                    }
                    firstServer = false;
                }
            }
        }
    }

    public void ConfirmDelete(string path)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Are you sure you wish to permanently DELETE:");
            Console.WriteLine($"{path}");
            Console.WriteLine("and its thumbnail? Press [y] to confirm or any other key to cancel");
            Console.ResetColor();
        }
    }

    public void ConfirmScreenshotCleanup()
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("The steam screenshots.vdf file is a steam file that contains screenshot metainformation from all steam games");
            Console.WriteLine("This operation will modify that file to remove any entries to Insight screenshots that no longer exists");
            Console.WriteLine("Please be aware that just because it works for me does not mean it will work for you");
            Console.WriteLine("*** For this to work, steam must be fully closed down ***");
            Console.WriteLine("At your own risk, press [y] three times to proceed or any other key to cancel");
            Console.ResetColor();
        }
    }

    public void WriteClosest(IEnumerable<DistanceModel> distanceModels, bool containsCetusInfo)
    {
        lock (_lock)
        {
            Console.WriteLine();
            Console.WriteLine("Closest puzzles");

            //Header
            Console.Write("Distance");
            Console.Write(" | ");
            Console.Write("Puzzle Id");
            Console.Write(" | ");
            Console.Write("Puzzle Type".PadLeft(18));
            Console.Write(" | ");
            Console.Write("Solved");
            if (containsCetusInfo)
            {
                Console.Write(" | ");
                Console.Write("Seen");
                Console.Write(" | ");
                Console.Write("Screenshot Requested");
            }
            Console.WriteLine();
            foreach (var worldPuzzle in distanceModels)
            {
                Console.Write($"{worldPuzzle.Distance/100:####}m".PadLeft(8));
                Console.Write(" | ");
                Console.Write($"{worldPuzzle.PuzzleId}".PadLeft(9));
                Console.Write(" | ");
                Console.Write($"{worldPuzzle.PuzzleName}".PadLeft(18));
                Console.Write(" | ");
                WriteYesNo(worldPuzzle.IsSolved, 6, false);

                if (containsCetusInfo)
                {
                    Console.Write(" | ");
                    WriteYesNo(worldPuzzle.Seen, 4, false);
                    Console.Write(" | ");
                    WriteYesNo(worldPuzzle.RequestsScreenshot, 0, true);
                }
                Console.WriteLine();
            }
        }
    }

    private void WriteYesNo(bool? value, int padding, bool invertColors)
    {
        if (value == null)
        {
            Console.Write("".PadLeft(padding));
        }
        else if (value == true)
        {
            Console.ForegroundColor = invertColors ? ConsoleColor.Red : ConsoleColor.Green;
            Console.Write("Yes".PadLeft(padding));
        }
        else
        {
            Console.ForegroundColor = invertColors ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write("No".PadLeft(padding));
        }
        Console.ResetColor();
    }

    public class WaypointMessage
    {
        public required int PuzzleId { get; init; }
        public required string PuzzleName { get; init; }
        public required int CurrentWaypoint { get; init; }
        public required int TotalWaypoints { get; init; }
        public bool? Stale { get; set; }
        public bool? IsOnCurrentServer { get; set; }
    }

    public void WriteWaypointMessage(WaypointMessage message)
    {
        lock (_lock)
        {
            Console.Write("Targeting: ");
            WritePuzzleName(message.PuzzleName);
            Console.Write($" with id {message.PuzzleId} ({message.CurrentWaypoint}/{message.TotalWaypoints})");
            if (message.Stale.HasValue)
            {
                Console.Write(" Stale: ");
                if (message.Stale == true)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Yes");
                    Console.ResetColor();
                }

                if (message.Stale == false)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("No");
                    Console.ResetColor();
                }
            }

            if (message.IsOnCurrentServer.HasValue)
            {
                Console.Write(" Server: ");
                if (message.IsOnCurrentServer == true)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("Current");
                    Console.ResetColor();
                }

                if (message.IsOnCurrentServer == false)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Other");
                    Console.ResetColor();
                }
            }
            Console.WriteLine();
        }
    }
}