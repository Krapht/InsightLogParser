﻿using System.Text.Json;
using InsightLogParser.Common.World;

namespace InsightLogParser.Common.PuzzleParser;

public class GamePuzzleHandler
{
    private PuzzleRoot? _parsedDb;

    public IReadOnlyDictionary<int, InsightPuzzle> PuzzleDatabase { get; internal set; } = new Dictionary<int, InsightPuzzle>();


    public async Task LoadAsync(string path)
    {
        var fs = File.OpenRead(path);
        await using (fs.ConfigureAwait(false))
        {
            _parsedDb = await JsonSerializer.DeserializeAsync<PuzzleRoot>(fs);
            if (_parsedDb?.Puzzles == null)
            {
                //If this is somehow null, do nothing and keep going with an empty db
                return;
            }

            PuzzleDatabase = _parsedDb.Puzzles
                .Select(ProcessPuzzle)
                .ToDictionary(x => x.KrakenId);
        }
    }

    private InsightPuzzle ProcessPuzzle(Puzzle arg)
    {
        var puzzleType = WorldInformation.GetPuzzleTypeByLogName(arg.PuzzleType);

        if (puzzleType == PuzzleType.LogicGrid)
        {
            return ProcessLogicGrid(arg);
        }

        var puzzleZone = PuzzleZone.Unknown;
        var isWorldPuzzle = false;
        var incompatible = Enumerable.Empty<int>();

        if (arg.Serialized != null)
        {
            var deserialized = JsonSerializer.Deserialize<SerializedInfo>(arg.Serialized);
            if (deserialized != null)
            {
                isWorldPuzzle = IsWorldPuzzle(puzzleType, deserialized);
                //Zone
                switch (deserialized.Zone)
                {
                    case 2:
                        puzzleZone = PuzzleZone.VerdantGlen;
                        break;
                    case 3: puzzleZone = PuzzleZone.LucentWaters;
                        break;
                    case 4: puzzleZone = PuzzleZone.AutumnFalls;
                        break;
                    case 5: puzzleZone = PuzzleZone.ShadyWildwood;
                        break;
                    case 6: puzzleZone = PuzzleZone.SereneDeluge;
                        break;
                    default: puzzleZone = PuzzleZone.Unknown;
                        break;
                }

                //Incompatible
                incompatible = (deserialized.IncompatibleKrakenIDs ?? "").Split("-").Select(value =>
                {
                    var isOk = int.TryParse(value, out var id);
                    return (IsOk: isOk, KrakenId: id);
                }).Where(x => x.IsOk).Select(x => x.KrakenId);
            }
        }

        return new InsightPuzzle()
        {
            KrakenId = arg.Pid,
            Zone = puzzleZone,
            Type = puzzleType,
            IsWorldPuzzle = isWorldPuzzle,
            IncompatibleIds = incompatible.ToList(),
        };
    }

    private PuzzleZone GetZoneFromName(string? name)
    {
        switch (name)
        {
            case "Zone1": return PuzzleZone.VerdantGlen;
            case "Zone2": return PuzzleZone.LucentWaters;
            case "Zone3": return PuzzleZone.AutumnFalls;
            case "Zone4": return PuzzleZone.ShadyWildwood;
            case "Zone5": return PuzzleZone.SereneDeluge;
            default:
                return PuzzleZone.Unknown;
        }
    }

    private InsightPuzzle ProcessLogicGrid(Puzzle puzzle)
    {
        var zone = PuzzleZone.Unknown;
        var type = PuzzleType.LogicGrid;
        var isWorldPuzzle = false;

        if (puzzle.Serialized != null) //Only music grids have serialized info
        {
            type = PuzzleType.MusicGrid;
            var deserialized = JsonSerializer.Deserialize<SerializedInfo>(puzzle.Serialized);
            if (deserialized != null)
            {
                zone = GetZoneFromName(deserialized.PoolName);
                isWorldPuzzle = (zone != PuzzleZone.Unknown);
            }
        }
        else
        {
            zone = GetZoneFromName(puzzle.Zone);
            isWorldPuzzle = (zone != PuzzleZone.Unknown);
            if (puzzle.PData != null)
            {
                var binary = Convert.FromBase64String(puzzle.PData);
                if (binary.Length >= 3)
                {
                    var subType = binary[2];
                    if (subType == 2) type = PuzzleType.PatternGrid;
                    if (subType == 12) type = PuzzleType.MemoryGrid;
                }
            }
        }
        return new InsightPuzzle()
        {
            KrakenId = puzzle.Pid,
            Zone = zone,
            Type = type,
            IsWorldPuzzle = isWorldPuzzle,
        };
    }

    public bool? IsWorldPuzzle(int puzzleId)
    {
        if (PuzzleDatabase.Count == 0) return null;
        return PuzzleDatabase.ContainsKey(puzzleId) && PuzzleDatabase[puzzleId].IsWorldPuzzle;
    }

    private bool IsWorldPuzzle(PuzzleType type, SerializedInfo deserialized)
    {
        switch (type)
        {
            case PuzzleType.Unknown:
            case PuzzleType.ArmillaryRings:
            case PuzzleType.Skydrop:
                return false;

            case PuzzleType.WanderingEcho:
            case PuzzleType.MatchBox:
            case PuzzleType.HiddenArchway:
            case PuzzleType.HiddenPentad:
            case PuzzleType.HiddenRing:
            case PuzzleType.FlowOrbs:
            case PuzzleType.HiddenCube:
            case PuzzleType.SightSeer:
            case PuzzleType.LightMotif:
            case PuzzleType.ShyAura:
            case PuzzleType.GlideRings:
                return deserialized.SpawnBehaviour == 2 && deserialized.Map == "BetaCampaign";

            case PuzzleType.SentinelStones:
            case PuzzleType.CrystalLabyrinth:
            case PuzzleType.MatchThree:
            case PuzzleType.RollingBlock:
            case PuzzleType.PhasicDial:
            case PuzzleType.MorphicFractal:
            case PuzzleType.ShiftingMosaic:
                return deserialized.PoolName == "live" && deserialized.UseInSandbox == true;

            //Grids are handled elsewhere
            case PuzzleType.MusicGrid:
            case PuzzleType.LogicGrid:
            case PuzzleType.PatternGrid:
            case PuzzleType.MemoryGrid:
                throw new ArgumentException("Grids should be handled separately");

            default:
                return false;
        }
    }
}