using System.Text.Json.Serialization;

namespace InsightLogParser.Common.ApiModels;

public class AuthResponse
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }
}