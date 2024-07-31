using InsightLogParser.Common.PuzzleParser;
using InsightLogParser.Common.World;

namespace InsightLogParser.Client;

internal class TeleportManager
{
    private readonly MessageWriter _writer;
    private DateTimeOffset _lastTeleportTime = DateTimeOffset.UtcNow;
    private Coordinate? _lastTeleport = null;
    private Coordinate? _target = null;
    private InsightPuzzle? _targetPuzzle = null;

    public TeleportManager(MessageWriter writer)
    {
        _writer = writer;
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
        _writer.WriteTeleportDebug($"Teleported to X: {destination.X:F0} Y: {destination.Y:F0} Z: {destination.Z:F0}");

        if (_target != null)
        {
            var targetType = _targetPuzzle?.Type ?? PuzzleType.Unknown;
            WriteDistance(_lastTeleport.Value, _target.Value, _writer, WorldInformation.GetPuzzleName(targetType));
        }
    }

    public static void WriteDistance(Coordinate current, Coordinate target, MessageWriter writer, string? puzzleType)
    {
        var delta = target - current;
        var distance = target.GetDistance3d(current);

        var xString = delta.X < 0 ? $"{-delta.X/100:####}m west" : $"{delta.X/100:####}m east";
        var yString = delta.Y < 0 ? $"{-delta.Y/100:####}m north" : $"{delta.Y/100:####}m south";
        var zString = delta.Z < 0 ? $"{-delta.Z/100:####}m down" : $"{delta.Z/100:####}m up";
        var targetTypeString = puzzleType != null ? $" {puzzleType}" : null;
        writer.WriteInfo($"Target{targetTypeString}: {distance/100:####}m ({xString}, {yString}, {zString})");
    }

    public Coordinate? GetLastTeleport()
    {
        return _lastTeleport;
    }

    public bool HasTarget()
    {
        return _target != null;
    }

    public void SetTarget(Coordinate target, InsightPuzzle puzzle)
    {
        _target = target;
        _targetPuzzle = puzzle;
    }

    public void ClearTarget()
    {
        _target = null;
        _targetPuzzle = null;
    }

    public void HandleSolved(InsightPuzzle puzzle)
    {
        //If we found our target, clear it
        if (_targetPuzzle?.KrakenId == puzzle.KrakenId)
        {
            ClearTarget();
        }

        // If we solved a puzzle, we're obviously at that puzzle, so let's move to it if we have a coordinate for it.
        // Light motifs and sightseers already "teleport" you so no need to do it twice
        if (puzzle.PrimaryCoordinate.HasValue
            && puzzle.Type != PuzzleType.LightMotif //Already triggers a teleport log entry
            && puzzle.Type != PuzzleType.SightSeer //Already triggers a teleport log entry
            && puzzle.Type != PuzzleType.WanderingEcho //Might end up quite far away from its coordinate
            && puzzle.Type != PuzzleType.GlideRings //Might end up quite far away from its coordinate
            //TODO: Think about also matchboxes, exclude? exclude if distance > x? use half-way point as teleport?
            )
        {
            Teleport(puzzle.PrimaryCoordinate!.Value);
        }
    }
}