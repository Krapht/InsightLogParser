﻿using InsightLogParser.Common.ApiModels;
using InsightLogParser.Common.World;

namespace InsightLogParser.Client.Cetus;

internal interface ICetusClient : IDisposable
{
    //Misc operations
    bool IsDummy();
    Task<bool> AuthenticateAsync(Guid playerId, string configurationCetusApiKey);
    Task<string?> GetSigninCodeAsync();

    //Get operations
    Task<ZoneStatisticsResponse?> GetZoneStatisticsAsync(PuzzleZone zone);
    Task<Sighting[]?> GetSightingsAsync(PuzzleZone zone, PuzzleType type);
    Task<ZoneUnsolvedResponse?> GetSightedUnsolved(PuzzleZone zone);
    Task<PuzzleScreenshotsDetails[]?> GetPuzzleScreenshots(int puzzleId);

    //Post operations
    Task<SeenResponse?> PostSeenAsync(PlayerReport request);
    Task<SolvedResponse?> PostSolvedAsync(PlayerReport request);
    Task<ScreenshotResponse?> PostScreenshotAsync(Screenshot screenshot);
    Task<ImportSaveResponse?> ImportSaveGameAsync(Stream saveGameStream);
    Task PublishPresence(PlayerPresence presence);

    //Delete operations
    Task ClearPresence();

    //"Search" operations
    Task<PuzzleStatusResponse?> GetPuzzleStatusAsync(PuzzleStatusRequest request);
}