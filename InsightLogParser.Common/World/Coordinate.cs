using System.Globalization;

namespace InsightLogParser.Common.World;

public readonly record struct Coordinate(float X, float Y, float Z)
{
    public static Coordinate? Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var parts = input.Split("|");
        var coordsPart = parts[0];
        var xyz = coordsPart.Split(",");
        var parsed = xyz.Select(x => float.Parse(x, CultureInfo.InvariantCulture)).ToArray();
        return new Coordinate(parsed[0], parsed[1], parsed[2]);
    }

    public static Coordinate operator -(Coordinate a, Coordinate b)
    {
        return new Coordinate(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }
}

public static class CoordinateExtensions
{
    public static double GetDistance2d(this Coordinate a, Coordinate b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public static double GetDistance3d(this Coordinate a, Coordinate b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        var dz = a.Z - b.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}