
namespace InsightLogParser.Common.World;

public class WorldInformation
{
    static WorldInformation()
    {
        //Verdant Glen
        var verdantPuzzles = new List<PuzzleInformation>
        {
            new() { Order = 1, PuzzleType = PuzzleType.MatchBox, TotalPuzzles = 274, CycleTime = new TimeOnly(13,29, 19) },
            new() { Order = 2, PuzzleType = PuzzleType.LightMotif, TotalPuzzles = 76, CycleTime = new TimeOnly(02,17,54) },
            new() { Order = 3, PuzzleType = PuzzleType.SightSeer, TotalPuzzles = 50, CycleTime = new TimeOnly(22, 25, 11) },
            new() { Order = 4, PuzzleType = PuzzleType.SentinelStones, TotalPuzzles = 52, CycleTime = new TimeOnly(02, 06, 03) },

            new() { Order = 5, PuzzleType = PuzzleType.HiddenRing, TotalPuzzles = 363, CycleTime = new TimeOnly(15, 47, 04) },
            new() { Order = 6, PuzzleType = PuzzleType.HiddenCube, TotalPuzzles = 229, CycleTime = new TimeOnly(02, 52, 52) },
            new() { Order = 7, PuzzleType = PuzzleType.HiddenArchway, TotalPuzzles = 353, CycleTime = new TimeOnly(13, 21, 13) },
            new() { Order = 8, PuzzleType = PuzzleType.HiddenPentad, TotalPuzzles = 50, CycleTime = new TimeOnly(19, 12, 52) },

            new() { Order = 9, PuzzleType = PuzzleType.LogicGrid, TotalPuzzles = 633, CycleTime = new TimeOnly(11, 17, 06) },
            new() { Order = 10, PuzzleType = PuzzleType.MemoryGrid, TotalPuzzles = 24, CycleTime = new TimeOnly(19, 34, 42) },
            new() { Order = 11, PuzzleType = PuzzleType.PatternGrid, TotalPuzzles = 111, CycleTime = new TimeOnly(16, 12, 33) },

            new() { Order = 12, PuzzleType = PuzzleType.WanderingEcho, TotalPuzzles = 93, CycleTime = new TimeOnly(18, 05, 38) },
            new() { Order = 13, PuzzleType = PuzzleType.GlideRings, TotalPuzzles = 16, CycleTime = new TimeOnly(11, 52, 48) },
            new() { Order = 14, PuzzleType = PuzzleType.FlowOrbs, TotalPuzzles = 35, CycleTime = new TimeOnly(05, 33, 00) },

            new() { Order = 15, PuzzleType = PuzzleType.CrystalLabyrinth, TotalPuzzles = 82, CycleTime = new TimeOnly(11, 27, 53) },
            new() { Order = 16, PuzzleType = PuzzleType.MorphicFractal, TotalPuzzles = 93, CycleTime = new TimeOnly(15, 55, 51) },

            new() { Order = 17, PuzzleType = PuzzleType.MatchThree, TotalPuzzles = 356, CycleTime = new TimeOnly(01, 16, 17) },
        };
        var verdantZone = new WorldZone
        {
            Zone = PuzzleZone.VerdantGlen,
            Name = "VERDANT GLEN",
            Puzzles = verdantPuzzles.ToDictionary(x => x.PuzzleType),
        };

        //Lucent Waters
        var lucentPuzzles = new List<PuzzleInformation>
        {
            new() { PuzzleType = PuzzleType.SightSeer, TotalPuzzles = 181, CycleTime = new TimeOnly(12, 44, 23) },
            new() { PuzzleType = PuzzleType.MatchBox, TotalPuzzles = 326, CycleTime = new TimeOnly(03, 48, 32) },
            new() { PuzzleType = PuzzleType.LightMotif, TotalPuzzles = 132, CycleTime = new TimeOnly(16, 37, 06) },

            new() { PuzzleType = PuzzleType.HiddenCube, TotalPuzzles = 191, CycleTime = new TimeOnly(17, 12, 05) },
            new() { PuzzleType = PuzzleType.HiddenArchway, TotalPuzzles = 363, CycleTime = new TimeOnly(03, 40, 27) },
            new() { PuzzleType = PuzzleType.HiddenRing, TotalPuzzles = 307, CycleTime = new TimeOnly(06, 06, 17) },
            new() { PuzzleType = PuzzleType.HiddenPentad, TotalPuzzles = 43, CycleTime = new TimeOnly(09, 32, 06) },

            new() { PuzzleType = PuzzleType.LogicGrid, TotalPuzzles = 640, CycleTime = new TimeOnly(01, 36, 18) },
            new() { PuzzleType = PuzzleType.PatternGrid, TotalPuzzles = 88, CycleTime = new TimeOnly(06, 31, 47) },
            new() { PuzzleType = PuzzleType.MemoryGrid, TotalPuzzles = 21, CycleTime = new TimeOnly(09, 53, 55) },
            new() { PuzzleType = PuzzleType.MusicGrid, TotalPuzzles = 23, CycleTime = new TimeOnly(16, 57, 02) },

            new() { PuzzleType = PuzzleType.WanderingEcho, TotalPuzzles = 117, CycleTime = new TimeOnly(08, 24, 51) },
            new() { PuzzleType = PuzzleType.GlideRings, TotalPuzzles = 32, CycleTime = new TimeOnly(02, 12, 01) },
            new() { PuzzleType = PuzzleType.FlowOrbs, TotalPuzzles = 70, CycleTime = new TimeOnly(19, 52, 13) },

            new() { PuzzleType = PuzzleType.MatchThree, TotalPuzzles = 89, CycleTime = new TimeOnly(15, 35, 29) },
            new() { PuzzleType = PuzzleType.PhasicDial, TotalPuzzles = 12, CycleTime = new TimeOnly(06, 15, 22) },

            new() { PuzzleType = PuzzleType.MorphicFractal, TotalPuzzles = 99, CycleTime = new TimeOnly(06, 15, 05) },
        };
        var lucentZone = new WorldZone
        {
            Zone = PuzzleZone.LucentWaters,
            Name = "LUCENT WATERS",
            Puzzles = lucentPuzzles.ToDictionary(x => x.PuzzleType),
        };

        //Autumn Falls
        var autumnPuzzles = new List<PuzzleInformation>
        {
            new() { PuzzleType = PuzzleType.MatchBox, TotalPuzzles = 415, CycleTime = new TimeOnly(18, 37, 14) },
            new() { PuzzleType = PuzzleType.SightSeer, TotalPuzzles = 202, CycleTime = new TimeOnly(03, 33, 07) },
            new() { PuzzleType = PuzzleType.LightMotif, TotalPuzzles = 194, CycleTime = new TimeOnly(07, 25, 51) },
            new() { PuzzleType = PuzzleType.SentinelStones, TotalPuzzles = 94, CycleTime = new TimeOnly(07, 14, 00) },

            new() { PuzzleType = PuzzleType.HiddenCube, TotalPuzzles = 189, CycleTime = new TimeOnly(08, 00, 49) },
            new() { PuzzleType = PuzzleType.HiddenArchway, TotalPuzzles = 537, CycleTime = new TimeOnly(18, 29, 11) },
            new() { PuzzleType = PuzzleType.HiddenRing, TotalPuzzles = 563, CycleTime = new TimeOnly(20, 55, 01) },
            new() { PuzzleType = PuzzleType.HiddenPentad, TotalPuzzles = 46, CycleTime = new TimeOnly(00, 20, 49) },

            new() { PuzzleType = PuzzleType.LogicGrid, TotalPuzzles = 612, CycleTime = new TimeOnly(16, 25, 02) },
            new() { PuzzleType = PuzzleType.MusicGrid, TotalPuzzles = 115, CycleTime = new TimeOnly(07, 45, 47) },
            new() { PuzzleType = PuzzleType.MemoryGrid, TotalPuzzles = 27, CycleTime = new TimeOnly(00, 42, 39) },
            new() { PuzzleType = PuzzleType.PatternGrid, TotalPuzzles = 20, CycleTime = new TimeOnly(21, 20, 29) },

            new() { PuzzleType = PuzzleType.FlowOrbs, TotalPuzzles = 93, CycleTime = new TimeOnly(10, 40, 57) },
            new() { PuzzleType = PuzzleType.WanderingEcho, TotalPuzzles = 55, CycleTime = new TimeOnly(23, 13, 34) },
            new() { PuzzleType = PuzzleType.GlideRings, TotalPuzzles = 64, CycleTime = new TimeOnly(17, 00, 43) },

            new() { PuzzleType = PuzzleType.RollingBlock, TotalPuzzles = 80, CycleTime = new TimeOnly(12, 19, 38) },
            new() { PuzzleType = PuzzleType.MatchThree, TotalPuzzles = 55, CycleTime = new TimeOnly(06, 24, 14) },
            new() { PuzzleType = PuzzleType.PhasicDial, TotalPuzzles = 185, CycleTime = new TimeOnly(21,04, 05) },
            new() { PuzzleType = PuzzleType.ShiftingMosaic, TotalPuzzles = 29, CycleTime = new TimeOnly(12, 36, 21) },

            new() { PuzzleType = PuzzleType.CrystalLabyrinth, TotalPuzzles = 81, CycleTime = new TimeOnly(16, 35, 49) },
            new() { PuzzleType = PuzzleType.MorphicFractal, TotalPuzzles = 84, CycleTime = new TimeOnly(21, 03, 48) },

        };
        var autumnZone = new WorldZone
        {
            Zone = PuzzleZone.AutumnFalls,
            Name = "AUTUMN FALLS",
            Puzzles = autumnPuzzles.ToDictionary(x => x.PuzzleType),
        };

        //Shady Wildwood
        var shadyPuzzles = new List<PuzzleInformation>
        {
            new() { PuzzleType = PuzzleType.MatchBox, TotalPuzzles = 338, CycleTime = new TimeOnly(20, 13, 23) },
            new() { PuzzleType = PuzzleType.LightMotif, TotalPuzzles = 137, CycleTime = new TimeOnly(09, 02, 00) },
            new() { PuzzleType = PuzzleType.SightSeer, TotalPuzzles = 186, CycleTime = new TimeOnly(05, 09, 17) },
            new() { PuzzleType = PuzzleType.SentinelStones, TotalPuzzles = 100, CycleTime = new TimeOnly(08, 50, 09) },

            new() { PuzzleType = PuzzleType.HiddenCube, TotalPuzzles = 291, CycleTime = new TimeOnly(09, 36, 58) },
            new() { PuzzleType = PuzzleType.HiddenRing, TotalPuzzles = 293, CycleTime = new TimeOnly(22, 31, 10) },
            new() { PuzzleType = PuzzleType.HiddenPentad, TotalPuzzles = 64, CycleTime = new TimeOnly(01, 56, 58) },
            new() { PuzzleType = PuzzleType.HiddenArchway, TotalPuzzles = 517, CycleTime = new TimeOnly(20, 05, 20) },

            new() { PuzzleType = PuzzleType.LogicGrid, TotalPuzzles = 590, CycleTime = new TimeOnly(18, 01, 11) },
            new() { PuzzleType = PuzzleType.MemoryGrid, TotalPuzzles = 111, CycleTime = new TimeOnly(02, 18, 48) },
            new() { PuzzleType = PuzzleType.MusicGrid, TotalPuzzles = 47, CycleTime = new TimeOnly(09, 21, 56) },
            new() { PuzzleType = PuzzleType.PatternGrid, TotalPuzzles = 20, CycleTime = new TimeOnly(22, 56, 39) },

            new() { PuzzleType = PuzzleType.WanderingEcho, TotalPuzzles = 89, CycleTime = new TimeOnly(00, 49, 44) },
            new() { PuzzleType = PuzzleType.FlowOrbs, TotalPuzzles = 54, CycleTime = new TimeOnly(12, 17, 05) },
            new() { PuzzleType = PuzzleType.GlideRings, TotalPuzzles = 35, CycleTime = new TimeOnly(18, 36, 53) },

            new() { PuzzleType = PuzzleType.RollingBlock, TotalPuzzles = 103, CycleTime = new TimeOnly(13, 55, 46) },
            new() { PuzzleType = PuzzleType.ShiftingMosaic, TotalPuzzles = 129, CycleTime = new TimeOnly(14, 12, 30) },
            new() { PuzzleType = PuzzleType.PhasicDial, TotalPuzzles = 50, CycleTime = new TimeOnly(22, 40, 14) },

            new() { PuzzleType = PuzzleType.ShyAura, TotalPuzzles = 207, CycleTime = new TimeOnly(14, 24, 46) },
            new() { PuzzleType = PuzzleType.CrystalLabyrinth, TotalPuzzles = 81, CycleTime = new TimeOnly(18, 11, 58) },
        };
        var shadyZone = new WorldZone
        {
            Zone = PuzzleZone.ShadyWildwood,
            Name = "SHADY WILDWOOD",
            Puzzles = shadyPuzzles.ToDictionary(x => x.PuzzleType),
        };

        //Serene Deluge
        var serenePuzzles = new List<PuzzleInformation>
        {
            new() { PuzzleType = PuzzleType.LightMotif, TotalPuzzles = 148, CycleTime = new TimeOnly(00, 55, 43) },
            new() { PuzzleType = PuzzleType.MatchBox, TotalPuzzles = 182, CycleTime = new TimeOnly(12, 07, 08) },
            new() { PuzzleType = PuzzleType.SightSeer, TotalPuzzles = 150, CycleTime = new TimeOnly(21, 03, 00) },
            new() { PuzzleType = PuzzleType.SentinelStones, TotalPuzzles = 109, CycleTime = new TimeOnly(00, 43, 53) },

            new() { PuzzleType = PuzzleType.HiddenPentad, TotalPuzzles = 62, CycleTime = new TimeOnly(17, 50, 42) },
            new() { PuzzleType = PuzzleType.HiddenCube, TotalPuzzles = 299, CycleTime = new TimeOnly(01, 30, 42) },
            new() { PuzzleType = PuzzleType.HiddenRing, TotalPuzzles = 400, CycleTime = new TimeOnly(14, 24, 53) },
            new() { PuzzleType = PuzzleType.HiddenArchway, TotalPuzzles = 200, CycleTime = new TimeOnly(11, 59, 04) },

            new() { PuzzleType = PuzzleType.LogicGrid, TotalPuzzles = 602, CycleTime = new TimeOnly(09, 54, 55) },
            new() { PuzzleType = PuzzleType.MusicGrid, TotalPuzzles = 116, CycleTime = new TimeOnly(01, 15, 39) },
            new() { PuzzleType = PuzzleType.MemoryGrid, TotalPuzzles = 37, CycleTime = new TimeOnly(18, 12, 32) },
            new() { PuzzleType = PuzzleType.PatternGrid, TotalPuzzles = 20, CycleTime = new TimeOnly(14, 50, 23) },

            new() { PuzzleType = PuzzleType.FlowOrbs, TotalPuzzles = 39, CycleTime = new TimeOnly(04, 10, 50) },
            new() { PuzzleType = PuzzleType.GlideRings, TotalPuzzles = 89, CycleTime = new TimeOnly(10, 30, 38) },
            new() { PuzzleType = PuzzleType.WanderingEcho, TotalPuzzles = 100, CycleTime = new TimeOnly(16, 43, 27) },

            new() { PuzzleType = PuzzleType.ShiftingMosaic, TotalPuzzles = 355, CycleTime = new TimeOnly(06, 06, 15) },
            new() { PuzzleType = PuzzleType.RollingBlock, TotalPuzzles = 130, CycleTime = new TimeOnly(05, 49, 31) },
            new() { PuzzleType = PuzzleType.PhasicDial, TotalPuzzles = 101, CycleTime = new TimeOnly(14, 33, 58) },

            new() { PuzzleType = PuzzleType.ShyAura, TotalPuzzles = 180, CycleTime = new TimeOnly(06, 18, 30) },
            new() { PuzzleType = PuzzleType.CrystalLabyrinth, TotalPuzzles = 78, CycleTime = new TimeOnly(10, 05, 42) },
        };
        var sereneZone = new WorldZone
        {
            Zone = PuzzleZone.SereneDeluge,
            Name = "SERENE DELUGE",
            Puzzles = serenePuzzles.ToDictionary(x => x.PuzzleType),
        };

        var zones = new List<WorldZone>
        {
            verdantZone,
            lucentZone,
            autumnZone,
            shadyZone,
            sereneZone,
        };

        Zones = zones.ToDictionary(x => x.Zone);
    }

    public static Dictionary<PuzzleZone, WorldZone> Zones;

    public static PuzzleType GetPuzzleTypeByLogName(string? logName)
    {
        switch (logName)
        {
            case "followTheShiny":
                return PuzzleType.WanderingEcho;
            case "matchbox":
                return PuzzleType.MatchBox;
            case "hiddenArchway":
                return PuzzleType.HiddenArchway;
            case "gyroRing":
                return PuzzleType.ArmillaryRings;
            case "logicGrid":
                return PuzzleType.LogicGrid;
            case "seek5":
                return PuzzleType.HiddenPentad;
            case "ryoanji":
                return PuzzleType.SentinelStones;
            case "hiddenRing":
                return PuzzleType.HiddenRing;
            case "rosary":
                return PuzzleType.Skydrop;
            case "mirrorMaze":
                return PuzzleType.CrystalLabyrinth;
            case "completeThePattern":
                return PuzzleType.PatternGrid;
            case "match3":
                return PuzzleType.MatchThree;
            case "racingBallCourse":
                return PuzzleType.FlowOrbs;
            case "hiddenCube":
                return PuzzleType.HiddenCube;
            case "viewfinder":
                return PuzzleType.SightSeer;
            case "rollingCube":
                return PuzzleType.RollingBlock;
            case "lockpick":
                return PuzzleType.PhasicDial;
            case "musicGrid":
                return PuzzleType.MusicGrid;
            case "lightPattern":
                return PuzzleType.LightMotif;
            case "ghostObject":
                return PuzzleType.ShyAura;
            case "memoryGrid":
                return PuzzleType.MemoryGrid;
            case "fractalMatch":
                return PuzzleType.MorphicFractal;
            case "racingRingCourse":
                return PuzzleType.GlideRings;
            case "klotski":
                return PuzzleType.ShiftingMosaic;
            default:
                return PuzzleType.Unknown;
        }
    }

    public static string GetLogName(PuzzleType type)
    {
        switch (type)
        {
            case PuzzleType.Unknown:
                return "UNKNOWN";
            case PuzzleType.WanderingEcho:
                return "followTheShiny";
            case PuzzleType.MatchBox:
                return "matchbox";
            case PuzzleType.HiddenArchway:
                return "hiddenArchway";
            case PuzzleType.ArmillaryRings:
                return "gyroRing";
            case PuzzleType.LogicGrid:
                return "logicGrid";
            case PuzzleType.HiddenPentad:
                return "seek5";
            case PuzzleType.SentinelStones:
                return "ryoanji";
            case PuzzleType.HiddenRing:
                return "hiddenRing";
            case PuzzleType.Skydrop:
                return "rosary";
            case PuzzleType.CrystalLabyrinth:
                return "mirrorMaze";
            case PuzzleType.PatternGrid:
                return "completeThePattern";
            case PuzzleType.MatchThree:
                return "match3";
            case PuzzleType.FlowOrbs:
                return "racingBallCourse";
            case PuzzleType.HiddenCube:
                return "hiddenCube";
            case PuzzleType.SightSeer:
                return "viewfinder";
            case PuzzleType.RollingBlock:
                return "rollingCube";
            case PuzzleType.PhasicDial:
                return "lockpick";
            case PuzzleType.MusicGrid:
                return "musicGrid";
            case PuzzleType.LightMotif:
                return "lightPattern";
            case PuzzleType.ShyAura:
                return "ghostObject";
            case PuzzleType.MemoryGrid:
                return "memoryGrid";
            case PuzzleType.MorphicFractal:
                return "fractalMatch";
            case PuzzleType.GlideRings:
                return "racingRingCourse";
            case PuzzleType.ShiftingMosaic:
                return "klotski";
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public static string GetPuzzleName(PuzzleType type)
    {
        switch (type)
        {
            case PuzzleType.Unknown:
                return "UNKNOWN";
            case PuzzleType.WanderingEcho:
                return "Wandering Echo";
            case PuzzleType.MatchBox:
                return "Matchbox";
            case PuzzleType.HiddenArchway:
                return "Hidden Archway";
            case PuzzleType.ArmillaryRings:
                return "Armillary Rings";
            case PuzzleType.LogicGrid:
                return "Logic Grid";
            case PuzzleType.HiddenPentad:
                return "Hidden Pentad";
            case PuzzleType.SentinelStones:
                return "Sentinel Stones";
            case PuzzleType.HiddenRing:
                return "Hidden Ring";
            case PuzzleType.Skydrop:
                return "Skydrop";
            case PuzzleType.CrystalLabyrinth:
                return "Crystal Labyrinth";
            case PuzzleType.PatternGrid:
                return "Pattern Grid";
            case PuzzleType.MatchThree:
                return "Match Three";
            case PuzzleType.FlowOrbs:
                return "Flow Orb";
            case PuzzleType.HiddenCube:
                return "Hidden Cube";
            case PuzzleType.SightSeer:
                return "Sightseer";
            case PuzzleType.RollingBlock:
                return "Rolling Block";
            case PuzzleType.PhasicDial:
                return "Phasic Dial";
            case PuzzleType.MusicGrid:
                return "Music Grid";
            case PuzzleType.LightMotif:
                return "Light Motif";
            case PuzzleType.ShyAura:
                return "Shy Aura";
            case PuzzleType.MemoryGrid:
                return "Memory Grid";
            case PuzzleType.MorphicFractal:
                return "Morphic Fractal";
            case PuzzleType.GlideRings:
                return "Glide Rings";
            case PuzzleType.ShiftingMosaic:
                return "Shifting Mosaic";
            default:
                return "UNKNOWN";
        }
    }

    public static PuzzleZone GetPuzzleZone(string? zoneName)
    {
        if (zoneName == null) return PuzzleZone.Unknown;
        switch (zoneName.ToLowerInvariant())
        {
            case "verdant glen":
                return PuzzleZone.VerdantGlen;
            case "lucent waters":
                return PuzzleZone.LucentWaters;
            case "autumn falls":
                return PuzzleZone.AutumnFalls;
            case "shady wildwood":
                return PuzzleZone.ShadyWildwood;
            case "serene deluge":
                return PuzzleZone.SereneDeluge;
            default:
                return PuzzleZone.Unknown;
        }
    }

    public static string GetZoneName(PuzzleZone zone)
    {
        switch (zone)
        {
            case PuzzleZone.Unknown:
                return "UNKNOWN";
            case PuzzleZone.VerdantGlen:
                return "Verdant Glen";
            case PuzzleZone.LucentWaters:
                return "Lucent Waters";
            case PuzzleZone.AutumnFalls:
                return "Autumn Falls";
            case PuzzleZone.ShadyWildwood:
                return "Shady Wildwood";
            case PuzzleZone.SereneDeluge:
                return "Serene Deluge";
            default:
                return "UNKNOWN";
        }
    }

    public static double GetDistance2d(Coordinate a, Coordinate b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public static double GetDistance3d(Coordinate a, Coordinate b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        var dz = a.Z - b.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public static IReadOnlyList<PuzzleInformation> GetPuzzlesInZone(PuzzleZone zone)
    {
        if (!Zones.TryGetValue(zone, out var zoneEntry)) return new List<PuzzleInformation>();
        return zoneEntry.Puzzles.Values
            .OrderBy(x => x.Order)
            .ToList();
    }

    public static TimeOnly? GetCycleTime(PuzzleZone zone, PuzzleType type)
    {
        if (!Zones.TryGetValue(zone, out var zoneEntry)) return null;
        if (!zoneEntry.Puzzles.TryGetValue(type, out var puzzleEntry)) return null;
        return puzzleEntry.CycleTime;
    }
}