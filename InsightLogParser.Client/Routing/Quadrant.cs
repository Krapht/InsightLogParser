using InsightLogParser.Common.World;

namespace InsightLogParser.Client.Routing;

internal class Quadrant
{
    public required string Name { get; init; }
    public required float MaxX { get; init; }
    public required float MinX { get; init; }
    public required float MaxY { get; init; }
    public required float MinY { get; init; }
    public required List<RouteNode> Nodes { get; init; }

    public double ClosestDistance(Coordinate to)
    {
        return Nodes.Min(x => x.Puzzle.PrimaryCoordinate!.Value.GetDistance3d(to));
    }
}