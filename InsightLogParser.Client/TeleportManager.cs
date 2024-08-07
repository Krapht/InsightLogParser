using InsightLogParser.Common.World;

namespace InsightLogParser.Client;

/// <summary>
/// Handles teleport events
/// </summary>
internal class TeleportManager
{
    private readonly MessageWriter _writer;
    private readonly TargetManager _targetManager;
    private DateTimeOffset _lastTeleportTime = DateTimeOffset.UtcNow;
    private Coordinate? _lastTeleport = null;

    public TeleportManager(MessageWriter writer
        , TargetManager targetManager
        )
    {
        _writer = writer;
        _targetManager = targetManager;
    }

    public void Teleport(Coordinate destination)
    {
        if (destination.X == 0 || destination.Y == 0 || destination.Z == 0)
        {
            _writer.WriteTeleportDebug("Ignored teleport to 0");
            return;
        }
        if ((DateTimeOffset.UtcNow - _lastTeleportTime).TotalMilliseconds < 1000)
        {
            _writer.WriteTeleportDebug("Teleport on cooldown");
            return;
        }

        _lastTeleport = destination;
        _lastTeleportTime = DateTimeOffset.UtcNow;

        _targetManager.MovePlayer(destination);
        _writer.WriteTeleportDebug($"Teleported to X: {destination.X:F0} Y: {destination.Y:F0} Z: {destination.Z:F0}");
    }
}
