
namespace InsightLogParser.Client;

internal class Beeper
{
    private readonly Configuration _configuration;

    public static int MinFrequency => 37;
    public static int MaxFrequency => 32767;
    public static int MinDuration => 50;
    public static int MaxDuration => 1000;
    public static int MinBeeps => 1;
    public static int MaxBeeps => 10;
    public static int MinDelay => 50;
    public static int MaxDelay => 1000;

    public Beeper(Configuration configuration)
    {
        _configuration = configuration;
    }

    public void BeepForNewParse()
    {
        if (!_configuration.BeepOnNewParse) return;
        DoTheBeep(_configuration.NewParseBeepFrequency, _configuration.NewParseBeepDuration);
    }

    public void BeepForKnownParse()
    {
        if (!_configuration.BeepOnKnownParse) return;
        DoTheBeep(_configuration.KnownParseBeepFrequency, _configuration.KnownParseBeepDuration);
    }

    public void BeepForOpeningSolvedPuzzle()
    {
        if (!_configuration.BeepOnOpeningSolvedPuzzle) return;
        DoTheBeep(_configuration.OpenSolvedPuzzleBeepFrequency, _configuration.OpenSolvedPuzzleBeepDuration);
    }

    public void BeepForMissingScreenshot()
    {
        if (!_configuration.BeepForMissingScreenshot) return;
        DoTheBeep(_configuration.MissingScreenshotBeepFrequency, _configuration.MissingScreenshotBeepFrequency);
    }

    public async Task BeepForAttentionAsync()
    {
        if (!_configuration.BeepForAttention) return;
        var count = _configuration.BeepForAttentionCount;
        if( count < MinBeeps) return; //No beeps
        if (count > MaxBeeps) return; //Too many beeps
        for (int i = 0; i < count; i++)
        {
            DoTheBeep(_configuration.BeepForAttentionFrequency, _configuration.BeepForAttentionDuration);
            if (i + 1 < count)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(_configuration.BeepForAttentionInterval)).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }
        }
    }

    private void DoTheBeep(int frequency, int duration)
    {
        //Ignore invalid frequencies
        if (frequency < MinFrequency) return;
        if (frequency > MaxFrequency) return;
        Console.Beep(frequency, duration);
    }
}