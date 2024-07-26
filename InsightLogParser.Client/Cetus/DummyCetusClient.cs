
using InsightLogParser.Common.ApiModels;
using InsightLogParser.Common.World;

namespace InsightLogParser.Client.Cetus
{
    internal class DummyCetusClient : ICetusClient
    {
        public bool IsDummy() => true;

        public Task<SolvedResponse?> PostSolvedAsync(PlayerReport request)
        {
            return Task.FromResult<SolvedResponse?>(null);
        }

        public Task<ScreenshotResponse?> PostScreenshotAsync(Screenshot screenshot)
        {
            return Task.FromResult<ScreenshotResponse?>(null);
        }

        public Task<ImportSaveResponse?> ImportSaveGameAsync(Stream saveGameStream)
        {
            return Task.FromResult<ImportSaveResponse?>(null);
        }

        public Task<PuzzleStatusResponse?> GetPuzzleStatusAsync(PuzzleStatusRequest request)
        {
            return Task.FromResult<PuzzleStatusResponse?>(null);
        }

        public Task<string?> GetSigninCodeAsync()
        {
            return Task.FromResult<string?>(null);
        }

        public Task<ZoneUnsolvedResponse?> GetSightedUnsolved(PuzzleZone zone)
        {
            return Task.FromResult<ZoneUnsolvedResponse?>(null);
        }

        public Task<PuzzleScreenshotsDetails[]?> GetPuzzleScreenshots(int puzzleId)
        {
            return Task.FromResult<PuzzleScreenshotsDetails[]?>(null);
        }

        public Task<SeenResponse?> PostSeenAsync(PlayerReport request)
        {
            return Task.FromResult<SeenResponse?>(null);
        }

        public Task<bool> AuthenticateAsync(Guid playerId, string? configurationCetusApiKey)
        {
            return Task.FromResult(true);
        }

        public Task<ZoneStatisticsResponse?> GetZoneStatisticsAsync(PuzzleZone zone)
        {
            return Task.FromResult<ZoneStatisticsResponse?>(null);
        }

        public Task<Sighting[]?> GetSightingsAsync(PuzzleZone zone, PuzzleType type)
        {
            return Task.FromResult<Sighting[]?>(null);
        }

        public void Dispose() { } //Noop
    }
}
