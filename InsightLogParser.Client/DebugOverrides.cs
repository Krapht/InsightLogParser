namespace InsightLogParser.Client
{
    internal class DebugOverrides
    {
        public static string JsonFile => "Puzzles.json";
        public static string LogFile => "IslandsofInsight.log";
        public static Guid PlayerId { get; } = new("9B61B51F-FAD5-4FEC-8C33-546EBBA28640");
        public static bool IsGameRunning => true;
    }
}
