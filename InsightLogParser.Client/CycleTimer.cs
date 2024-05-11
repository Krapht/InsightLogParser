using System.Linq;
using InsightLogParser.Common;
using InsightLogParser.Common.World;

namespace InsightLogParser.Client;

internal class CycleTimer
{
    private readonly MessageWriter _messageWriter;
    private readonly TimeTools _timeTools;
    private readonly CancellationTokenSource _tokenSource;

    private Task _timerTask = Task.CompletedTask;

    internal static async Task<CycleTimer> CreateAndStartAsync(MessageWriter writer, TimeTools timeTools)
    {
        var timer = new CycleTimer(writer, timeTools);
        await timer.StartAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        return timer;
    }

    private CycleTimer(MessageWriter messageWriter, TimeTools timeTools)
    {
        _tokenSource = new();
        _messageWriter = messageWriter;
        _timeTools = timeTools;
    }

    private async Task StartAsync()
    {
        _messageWriter.WriteInitLine("Starting cycle tracking timer", ConsoleColor.Green);
        await Task.Yield();
        _timerTask = StartTimerAsync();
    }

    public async Task StopAsync()
    {
        _messageWriter.WriteDebug("Stopping CycleTimer");
        await _tokenSource.CancelAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        await _timerTask.ConfigureAwait(ConfigureAwaitOptions.None);

        _tokenSource.Dispose();
        _messageWriter.WriteDebug("CycleTimer stopped");
    }

    private async Task StartTimerAsync()
    {
        while (!_tokenSource.IsCancellationRequested)
        {
            var nextReset = WorldInformation.Zones.Values.SelectMany(x => x.Puzzles.Values, (x, y)
                    => (x.Zone, y.PuzzleType, TimeToReset: _timeTools.GetTimeTo(y.CycleTime)))
                .OrderBy(x => x.TimeToReset)
                .First(x => x.TimeToReset.TotalSeconds > 1); //Ensure we're not setting up a timer for the same reset

            var zoneName = WorldInformation.GetZoneName(nextReset.Zone);
            var puzzleName = WorldInformation.GetPuzzleName(nextReset.PuzzleType);
            _messageWriter.WriteNextCycle(zoneName, puzzleName, nextReset.TimeToReset);

            using (var timer = new PeriodicTimer(nextReset.TimeToReset))
            {
                try
                {
                    var fired = await timer.WaitForNextTickAsync(_tokenSource.Token).ConfigureAwait(false);
                    if (fired)
                    {
                        _messageWriter.WriteCycled(zoneName, puzzleName);
                    }
                }
                catch (OperationCanceledException)
                {
                }
            }
        }
        _messageWriter.WriteStopMessage("Cycle tracking timer stopped", ConsoleColor.Green);
    }
}