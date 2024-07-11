using InsightLogParser.Common.World;

namespace InsightLogParser.Client;

internal class TeleportManager
{
    private readonly MessageWriter _writer;
    private DateTimeOffset _lastTeleportTime = DateTimeOffset.UtcNow;
    private Coordinate? _lastTeleport = null;
    private Coordinate? _target = null;

    public TeleportManager(MessageWriter writer)
    {
        _writer = writer;
    }

    public void Teleport(Coordinate destination)
    {
        if (destination.X == 0 || destination.Y == 0 || destination.Z == 0)
        {
            _writer.WriteDebug("Ignored teleport to 0");
            return;
        }
        if ((DateTimeOffset.UtcNow - _lastTeleportTime).TotalMilliseconds < 1000)
        {
            _writer.WriteDebug("Teleport on cooldown");
            return;
        }

        _lastTeleport = destination;
        _lastTeleportTime = DateTimeOffset.UtcNow;
        _writer.WriteDebug($"Teleported to X: {destination.X:F0} Y: {destination.Y:F0} Z: {destination.Z:F0}");

        if (_target != null)
        {
            WriteDistance(_lastTeleport.Value, _target.Value, _writer);
        }
    }

    public static void WriteDistance(Coordinate current, Coordinate target, MessageWriter writer)
    {
        var delta = target - current;
        var distance = target.GetDistance3d(current);

        var xString = delta.X < 0 ? $"{-delta.X/100:####}m west" : $"{delta.X/100:####}m east";
        var yString = delta.Y < 0 ? $"{-delta.Y/100:####}m north" : $"{delta.Y/100:####}m south";
        var zString = delta.Z < 0 ? $"{-delta.Z/100:####}m down" : $"{delta.Z/100:####}m up";

        writer.WriteInfo($"Target: {distance/100:####}m ({xString}, {yString}, {zString})");
    }

    public Coordinate? GetLastTeleport()
    {
        return _lastTeleport;
    }

    public void SetTarget(Coordinate target)
    {
        _target = target;
    }
}