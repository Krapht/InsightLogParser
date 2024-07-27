
namespace InsightLogParser.Common;

public static class TimeExtensions
{
    public static string ToIso8601(this DateTimeOffset timestamp)
    {
        return $"{timestamp:yyyy-MM-dd HH:mm:ss} UTC";
    }

    public static string ToIso8601Date(this DateTimeOffset timestamp)
    {
        return $"{timestamp:yyyy-MM-dd} UTC";
    }

    public static string ToHHMMSS(this TimeSpan span, bool useShort)
    {
        if (useShort)
        {
            return span.ToString("hh\\:mm\\:ss");
        }

        if (span.Days > 0)
        {
            if (span.Days > 7)
            {
                return $"{span.Days} days and {span.Hours} hours";
            }
            return $"{span.Days} days, {span.Hours} hours and {span.Minutes} minutes";
        }

        return $"{span.Hours} hours, {span.Minutes} minutes and {span.Seconds} seconds";
    }
}