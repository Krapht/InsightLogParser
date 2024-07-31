using System.Linq;
using System.Text.Json.Serialization;
using InsightLogParser.Common.World;

namespace InsightLogParser.Common.ApiModels
{
    public class ImportSolve
    {
        [JsonPropertyName("puzzleId")]
        public int PuzzleId { get; set; }

        [JsonPropertyName("puzzleType")]
        public PuzzleType PuzzleType { get; set; }

        [JsonPropertyName("puzzleZone")]
        public PuzzleZone PuzzleZone { get; set; }

        [JsonPropertyName("solvedAt")]
        public DateTimeOffset SolvedAt { get; set; }
    }

    public class ImportSaveResponse
    {
        [JsonPropertyName("allSolves")]
        public int[] AllSolves { get; set; }

        [JsonPropertyName("mostRecentSolve")]
        public DateTimeOffset MostRecentSolve { get; set; }

        [JsonPropertyName("serverAddedCount")]
        public int ServerAddedCount { get; set; }

        [JsonPropertyName("serverRemovedCount")]
        public int ServerRemovedCount { get; set; }

        [JsonPropertyName("importedSolves")]
        public ImportSolve[] ImportedSolves { get; set; }

        [JsonPropertyName("removedStaleSolves")]
        public ImportSolve[] RemovedStales { get; set; } = [];

        [JsonPropertyName("serverAddedFlowOrbTimes")]
        public int ServerAddedFlowOrbTimes { get; set; }

        [JsonPropertyName("serverUpdatedFlowOrbTimes")]
        public int ServerUpdatedFlowOrbTimes { get; set; }
    }
}
