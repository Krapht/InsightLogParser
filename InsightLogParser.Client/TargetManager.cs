using System.Linq;
using InsightLogParser.Client.Websockets;
using InsightLogParser.Common.PuzzleParser;
using InsightLogParser.Common.World;

namespace InsightLogParser.Client;

internal class TargetManager
{
    private readonly MessageWriter _writer;
    private readonly ISocketUiCommands _socketUiCommands;

    private Coordinate _lastPosition;
    private Coordinate? _target = null;
    private InsightPuzzle? _targetPuzzle = null;


    public TargetManager(MessageWriter writer
        , ISocketUiCommands socketUiCommands
        )
    {
        _writer = writer;
        _socketUiCommands = socketUiCommands;
    }

    public void MovePlayer(Coordinate destination)
    {
        _lastPosition = destination;
        _socketUiCommands.MovePlayer(destination);

        if (_target != null)
        {
            var targetType = _targetPuzzle?.Type ?? PuzzleType.Unknown;
            WriteDistance(_lastPosition, _target.Value, _writer, WorldInformation.GetPuzzleName(targetType));
        }
    }

    public Coordinate GetLastPosition()
    {
        return _lastPosition;
    }

    public bool HasLastPosition()
    {
        return _lastPosition != default;
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
            MovePlayer(puzzle.PrimaryCoordinate!.Value);
        }
    }
}