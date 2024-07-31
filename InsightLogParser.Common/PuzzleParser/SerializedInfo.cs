using System.Text.Json;
using System.Text.Json.Serialization;

namespace InsightLogParser.Common.PuzzleParser;

public class SerializedSubComponent
{
    [JsonPropertyName("RelativeTransform")]
    public string? RelativeTransform { get; set; }

    [JsonPropertyName("bUseForDungeonIdentification")]
    public bool UseForDungeonIdentification { get; set; }

    [JsonPropertyName("acceptAllByDefault")]
    public bool AcceptAllByDefault { get; set; }

    [JsonPropertyName("bBlockSpawning")]
    public bool BlockSpawning { get; set; }

    [JsonPropertyName("acceptedTypes")]
    public string? AcceptedTypes { get; set; }

    [JsonPropertyName("WorldTransform")]
    public string? WorldTransform { get; set; }

    [JsonPropertyName("Box")]
    public string? Box { get; set; }
}

public class SerializedInfo
{
    [JsonPropertyName("AdditionalBinaryData")]
    public string? AdditionalBinaryData { get; set; }

    [JsonPropertyName("BinaryData")]
    public string? BinaryData { get; set; }

    [JsonPropertyName("PuzzleClass")]
    public string? PuzzleClass { get; set; }

    [JsonPropertyName("SpawnBehaviour")]
    public int SpawnBehaviour { get; set; }

    [JsonPropertyName("SolveValue")]
    public int SolveValue { get; set; }

    [JsonPropertyName("CanAwardAutoQuest")]
    public bool CanAwardAutoQuest { get; set; }

    [JsonPropertyName("Disabled")]
    public bool Disabled { get; set; }

    [JsonPropertyName("AwakenIfAlwaysSpawn")]
    public bool AwakenIfAlwaysSpawn { get; set; }

    [JsonPropertyName("PuzzleType")]
    public string? PuzzleType { get; set; }

    [JsonPropertyName("LocalID")]
    public string? LocalID { get; set; }

    [JsonPropertyName("ActorTransform")]
    public string? ActorTransform { get; set; }

    [JsonPropertyName("Zone")]
    public int Zone { get; set; }

    [JsonPropertyName("Map")]
    public string? Map { get; set; }

    [JsonPropertyName("IncompatibleKrakenIDs")]
    public string? IncompatibleKrakenIDs { get; set; }

    [JsonPropertyName("SERIALIZEDSUBCOMP_PuzzleBounds-0")]
    public JsonElement? SubComponent0 { get; set; }

    [JsonPropertyName("SERIALIZEDSUBCOMP_PuzzleBounds-1")]
    public JsonElement? SubComponent1 { get; set; }

    [JsonPropertyName("ghostType")]
    public int? GhostType { get; set; }

    [JsonPropertyName("forceTutorialColors")]
    public bool? ForceTutorialColors { get; set; }

    [JsonPropertyName("KID")]
    public int KID { get; set; }

    [JsonPropertyName("PoolName")]
    public string? PoolName { get; set; }

    [JsonPropertyName("bLive")]
    public bool? IsLive { get; set; }

    [JsonPropertyName("bUseInSandbox")]
    public bool? UseInSandbox { get; set; }

    [JsonPropertyName("SpawnRadius")]
    public int? SpawnRadius { get; set; }

    [JsonPropertyName("ManualPlacement")]
    public bool? ManualPlacement { get; set; }

    [JsonPropertyName("StartingPlatformTransform")]
    public string? StartingPlatformTransform { get; set; }

    [JsonPropertyName("Mesh1Transform")]
    public string? Mesh1Transform { get; set; }

    [JsonPropertyName("Mesh2Transform")]
    public string? Mesh2Transform { get; set; }

    [JsonPropertyName("shinyMeshTransform")]
    public string? ShinyMeshTransform { get; set; }

    [JsonPropertyName("SandboxMilestones")]
    public string? SandboxMilestones { get; set; }
}