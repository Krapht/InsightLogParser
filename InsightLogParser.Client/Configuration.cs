
namespace InsightLogParser.Client;

public class Configuration
{
    public static readonly int CurrentConfigurationVersion = 4;

    public int ConfigVersion { get; set; } = CurrentConfigurationVersion;

    //Paths
    public string? ForceLogFolder { get; set; }
    public string? ForceGameRootFolder { get; set; }
    public string? ForcedParsedDatabasePath { get; set; }
    public string? ForceScreenshotFolder { get; set; }
    public string? ForcedOfflineSaveFile { get; set; }

    //Screenshots
    public bool MonitorScreenshots { get; set; } = true;
    public bool ConfirmScreenshotDelete { get; set; } = true;
    public bool DeleteOnQuickUpload { get; set; } = false;

    //Cetus
    public string? CetusApiKey { get; set; }
    public Guid? CetusPlayerId { get; set; }
    public string? CetusUri { get; set; }

    //Routes
    public bool ExcludeStalePuzzlesInRoutes { get; set; } = false;

    //Misc
    public bool ShowGameLogLines { get; set; } = false;
    public bool DebugMode { get; set; } = false;

    //Beeps
    public bool BeepOnNewParse { get; set; } = true;
    public int NewParseBeepFrequency { get; set; } = 880;
    public int NewParseBeepDuration { get; set; } = 100;
    public bool BeepOnKnownParse { get; set; } = false;
    public int KnownParseBeepFrequency { get; set; } = 220;
    public int KnownParseBeepDuration { get; set; } = 200;
    public bool BeepOnOpeningSolvedPuzzle { get; set; } = true;
    public int OpenSolvedPuzzleBeepFrequency { get; set; } = 220;
    public int OpenSolvedPuzzleBeepDuration { get; set; } = 200;
    public bool BeepForAttention { get; set; } = true;
    public int BeepForAttentionFrequency { get; set; } = 220;
    public int BeepForAttentionDuration { get; set; } = 200;
    public int BeepForAttentionCount { get; set; } = 3;
    public int BeepForAttentionInterval { get; set; } = 200;
    public bool BeepForMissingScreenshot { get; set; }
    public int MissingScreenshotBeepFrequency { get; set; } = 440;
    public int MissingScreenshotBeepDuration { get; set; } = 150;
}