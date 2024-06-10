using System.Diagnostics;
using InsightLogParser.Common.World;

namespace InsightLogParser.Client.Routing;

internal class PuzzleRouter
{
    private const int MaxPuzzlesPerQuadrant = 10;

    private readonly MessageWriter _writer;
    private List<RouteNode> _sequence = [];
    private int? _sequenceIndex;
    private HashSet<int> _solvedForCurrentRoute = new();

    public PuzzleRouter(MessageWriter writer)
    {
        _writer = writer;
    }

    private IEnumerable<Quadrant> Divide(Quadrant outer)
    {
        if (outer.Nodes.Count == 0)
        {
            yield break; //Ignore empty quadrants
        }
        if (outer.Nodes.Count <= MaxPuzzlesPerQuadrant)
        {
            yield return outer; //No need to divide further
            yield break;
        }

        _writer.WriteDebug($"Breaking down quadrant {outer.Name} with {outer.Nodes.Count} puzzles");
        var midX = (outer.MaxX + outer.MinX) / 2;
        var midY = (outer.MaxY + outer.MinY) / 2;

        var quadrants = new List<Quadrant>
        {
            //Q1 - Top Right
            new Quadrant
            {
                Name = outer.Name + "-Q1",
                Nodes = outer.Nodes.Where(x => x.Puzzle.PrimaryCoordinate!.Value.X >= midX && x.Puzzle.PrimaryCoordinate!.Value.Y >= midY).ToList(),
                MaxX = outer.MaxX,
                MinX = midX,
                MaxY = outer.MaxY,
                MinY = midY,
            },
            //Q2 - Top Left
            new Quadrant
            {
                Name = outer.Name + "-Q2",
                Nodes = outer.Nodes.Where(x => x.Puzzle.PrimaryCoordinate!.Value.X < midX && x.Puzzle.PrimaryCoordinate!.Value.Y >= midY).ToList(),
                MaxX = midX,
                MinX = outer.MinX,
                MaxY = outer.MaxY,
                MinY = midY,
            },
            //Q3 - Bottom Left
            new Quadrant
            {
                Name = outer.Name + "-Q3",
                Nodes = outer.Nodes.Where(x => x.Puzzle.PrimaryCoordinate!.Value.X < midX && x.Puzzle.PrimaryCoordinate!.Value.Y < midY).ToList(),
                MaxX = midX,
                MinX = outer.MinX,
                MaxY = midY,
                MinY = outer.MinY,
            },
            //Q4 - Bottom Right
            new Quadrant
            {
                Name = outer.Name + "-Q4",
                Nodes = outer.Nodes.Where(x => x.Puzzle.PrimaryCoordinate!.Value.X >= midX && x.Puzzle.PrimaryCoordinate!.Value.Y < midY).ToList(),
                MaxX = outer.MaxX,
                MinX = midX,
                MaxY = midY,
                MinY = outer.MinY,
            },
        };
        foreach (var quadrant in quadrants)
        {
            foreach (var result in Divide(quadrant))
            {
                yield return result;
            }
        }
    }

    private List<RouteNode> Solve(Quadrant quadrant, Coordinate startCoordinate)
    {
        var closest = quadrant.Nodes.MinBy(x => x.Puzzle.PrimaryCoordinate!.Value.GetDistance3d(startCoordinate));
        var start = new List<RouteNode>() { closest };
        var remain = quadrant.Nodes.Except([closest]);
        var result = new List<List<RouteNode>>();
        Travel(result, start, remain.ToList());

        var first = result.First();
        if (result.Count == 1) return first;

        (double Length, Coordinate Previous) CalculateLength((double Length, Coordinate Previous) previous, RouteNode next)
        {
            var nextCoord = next.Puzzle.PrimaryCoordinate!.Value;
            return (Length: previous.Length + previous.Previous.GetDistance3d(nextCoord), Previous: nextCoord);
        }

        var bestPath = result
            .Select(path => (List: path, Distance: path.Skip(1).Aggregate((Length: 0D, Previous: path.First().Puzzle.PrimaryCoordinate!.Value), CalculateLength)))
            .MinBy(x => x.Distance)
            .List;
        return bestPath;
    }

    private void Travel<T>(List<List<T>> result, List<T> current, List<T> remain)
    {
        if (remain.Count == 0)
        {
            result.Add(current);
            return;
        }
        foreach (var node in remain)
        {
            var newRemain = remain.Except([node]).ToList();
            var newCurrent = current.ToList();
            newCurrent.Add(node);
            Travel(result, newCurrent, newRemain);
        }
    }

    public (RouteNode Node, int Index, int Max)? SetRoute(IEnumerable<RouteNode> puzzles, Coordinate startCoordinate)
    {
        var valid = puzzles
            .Where(x => x.Puzzle.PrimaryCoordinate != null)
            .ToList();

        if (valid.Count == 0)
        {
            _writer.WriteError("No valid puzzles for route");
            return null;
        }

        var timer = Stopwatch.StartNew();
        _writer.WriteInfo($"Calculating route for {valid.Count} puzzles...");

        var initial = new Quadrant()
        {
            Name = "Q",
            Nodes = valid,
            MaxX = valid.Max(x => x.Puzzle.PrimaryCoordinate!.Value.X),
            MinX = valid.Min(x => x.Puzzle.PrimaryCoordinate!.Value.X),
            MaxY = valid.Max(x => x.Puzzle.PrimaryCoordinate!.Value.Y),
            MinY = valid.Min(x => x.Puzzle.PrimaryCoordinate!.Value.Y),
        };

        var quadrants = Divide(initial).ToList();
        foreach (var quadrant in quadrants)
        {
            _writer.WriteDebug($"{quadrant.Name}: {quadrant.Nodes.Count}");
        }
        var startQuadrant = quadrants.MinBy(x => x.ClosestDistance(startCoordinate))!;
        quadrants.Remove(startQuadrant);
        _writer.WriteDebug($"Starting at {startQuadrant?.Name}");

        var fullPath = Solve(startQuadrant, startCoordinate);
        while (quadrants.Any())
        {
            var lastCoordinate = fullPath.Last().Puzzle.PrimaryCoordinate!.Value;
            var nextQuadrant = quadrants.MinBy(x => x.ClosestDistance(lastCoordinate))!;
            quadrants.Remove(nextQuadrant);
            fullPath.AddRange(Solve(nextQuadrant, lastCoordinate));
        }
        timer.Stop();
        var distance = fullPath.Aggregate((Length: 0D, Previous: startCoordinate), (previous, next) =>
        {
            var nextCoord = next.Puzzle.PrimaryCoordinate!.Value;
            return (Length: previous.Length + previous.Previous.GetDistance3d(nextCoord), Previous: nextCoord);
        }).Length / 100;
        _writer.WriteInfo($"Done in {timer.ElapsedMilliseconds} ms, total route length is {distance:F0}m from last teleport");

        _sequence = fullPath;
        _sequenceIndex = -1;
        _solvedForCurrentRoute = new HashSet<int>();
        return NextNode();
    }

    public void ClearRoute()
    {
        _sequence = [];
        _sequenceIndex = null;
        _solvedForCurrentRoute = new HashSet<int>();
    }

    public bool HasRoute()
    {
        return _sequenceIndex != null;
    }

    public (RouteNode Node, int Index, int Max)? PreviousNode()
    {
        var newIndex = _sequenceIndex - 1;
        if (newIndex < 0)
        {
            _writer.WriteError("No previous puzzle");
            return null;
        }

        _sequenceIndex = newIndex;
        return CurrentNode();
    }

    public (RouteNode Node, int Index, int Max)? CurrentNode()
    {
        if (_sequenceIndex == null || _sequenceIndex < 0)
        {
            _writer.WriteError("No current route");
            return null;
        }

        if (_sequenceIndex >= _sequence.Count)
        {
            _writer.WriteDebug("SequenceIndex out of bounds");
            return null;
        }
        return (Node: _sequence[_sequenceIndex.Value], Index: _sequenceIndex.Value + 1, _sequence.Count);
    }

    public (RouteNode Node, int Index, int Max)? NextNode()
    {
        if (_sequenceIndex == null)
        {
            _writer.WriteError("No current route");
        }

        var newIndex = _sequenceIndex + 1;
        if (newIndex >= _sequence.Count)
        {
            _writer.WriteInfo("No more puzzles in route");
            return null;
        }

        _sequenceIndex = newIndex;

        var current = CurrentNode();
        if (current != null && _solvedForCurrentRoute.Contains(current.Value.Node.Puzzle.KrakenId))
        {
            _writer.WriteInfo($"Skipping puzzle {current.Value.Node.Puzzle.KrakenId} as it has been solved already");
            return NextNode();
        }

        return current;
    }

    public void AddSolved(int puzzleId)
    {
        if (!HasRoute()) return;
        _solvedForCurrentRoute.Add(puzzleId);
    }
}