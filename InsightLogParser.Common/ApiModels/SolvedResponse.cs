using System.Text.Json.Serialization;
using InsightLogParser.Common.Screenshots;

namespace InsightLogParser.Common.ApiModels;

public class SolvedResponse
{
    [JsonPropertyName("firstSighting")]
    public required bool FirstSighting { get; set; }

    [JsonPropertyName("sightings")]
    public Sighting[]? Sightings { get; set; }

    [JsonPropertyName("screenshotCategories")]
    public ScreenshotCategory[] ScreenshotCategories { get; set; }
}