namespace InsightLogParser.Common.Screenshots;

public enum ScreenshotCategory
{
    //SentinelStones, WanderingEcho, CrystalLabyrinth
    Other = 0,

    /// <summary>
    /// Applicable to LogicGrid, PatternGrid, MatchThree, RollingBlock, PhasicDial, MorphicFractal, ShiftingMosaic
    /// </summary>
    Initial = 1,

    /// <summary>
    /// Applicable to LogicGrid, PatternGrid, SightSeer, MusicGrid, LightMotif, MemoryGrid, ShiftingMosaic
    /// </summary>
    Solved = 2,

    /// <summary>
    /// Applicable to HiddenArchway, HiddenPentad, HiddenRing, FlowOrbs, HiddenCube, ShyAura, GlideRings, Matchbox (both boxes)
    /// </summary>
    Location = 3,

    /// <summary>
    /// Applicable to FlowOrbs
    /// </summary>
    Scenic = 4,
}
