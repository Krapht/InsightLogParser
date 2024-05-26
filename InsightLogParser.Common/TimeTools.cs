
namespace InsightLogParser.Common;

public class TimeTools
{
    private readonly TimeProvider _timeProvider;

    public TimeTools(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public TimeSpan GetTimeTo(TimeOnly targetTime)
    {
        var nowTime = _timeProvider.GetUtcNow();

        var timeToTarget = targetTime.ToTimeSpan() - nowTime.TimeOfDay;

        //If the time was earlier than current time, add a day to make it tomorrow
        if (timeToTarget < TimeSpan.Zero)
        {
            timeToTarget = timeToTarget.Add(TimeSpan.FromDays(1));
        }
        return timeToTarget;
    }

    public DateTimeOffset GetCurrentCycleStartTime(TimeOnly cycleTime)
    {
        var timeToCycle = GetTimeTo(cycleTime);
        var timeSinceCycle = timeToCycle.Add(TimeSpan.FromHours(-24));
        var cycleStartTime = _timeProvider.GetUtcNow() + timeSinceCycle;
        return cycleStartTime;
    }

    public DateTimeOffset GetCurrentCycleEndTime(TimeOnly cycleTime)
    {
        var timeToCycle = GetTimeTo(cycleTime);
        return _timeProvider.GetUtcNow().Add(timeToCycle);
    }

    public TimeSpan GetTimeSince(DateTimeOffset agoTime)
    {
        return _timeProvider.GetUtcNow() - agoTime;
    }

    public string FormatTimeSince(DateTimeOffset? agoTime, string noDateVersion, bool useShort)
    {
        if (agoTime == null) return noDateVersion;
        if (agoTime.Value == default) return noDateVersion;

        return (_timeProvider.GetUtcNow() - agoTime.Value).ToHHMMSS(useShort);
    }
}