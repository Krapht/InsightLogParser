using InsightLogParser.Common.ApiModels;
using InsightLogParser.Common.World;

namespace InsightLogParser.Client.Cetus;

internal interface ICetusClient : IDisposable
{
    //Misc operations
    bool IsDummy();
    Task<bool> AuthenticateAsync(Guid playerId, string configurationCetusApiKey);

    //Get operations
    Task<ZoneStatisticsResponse?> GetZoneStatisticsAsync(PuzzleZone zone);
    Task<Sighting[]?> GetSightingsAsync(PuzzleZone zone, PuzzleType type);

    //Post operations
    Task<SeenResponse?> PostSeenAsync(PlayerReport request);
    Task<SolvedResponse?> PostSolvedAsync(PlayerReport request);
    Task<ScreenshotResponse?> PostScreenshotAsync(Screenshot screenshot);
    Task<string?> GetSigninCodeAsync();
}