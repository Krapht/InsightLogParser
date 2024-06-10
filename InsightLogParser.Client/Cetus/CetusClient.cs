using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using InsightLogParser.Common.ApiModels;
using InsightLogParser.Common.World;

namespace InsightLogParser.Client.Cetus;

internal class AuthResult
{
    [JsonPropertyName("accessToken")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("expiresIn")]
    public int ExpiresIn { get; set; }
}

internal class CetusClient : ICetusClient
{
    private readonly MessageWriter _messageWriter;
    private readonly HttpClient _httpClient;
    private DateTimeOffset? _tokenValid;
    private string? _basicAuth;

    public bool IsDummy() => false;

    public CetusClient(string baseUri, MessageWriter messageWriter)
    {
        _messageWriter = messageWriter;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUri)
        };
    }

    public async Task<bool> RefreshAuthIfNeeded()
    {
        if (_basicAuth == null ||  _tokenValid == null) return false;
        if (DateTimeOffset.UtcNow.AddMinutes(1) < _tokenValid.Value) return true;

        return await RefreshAuthAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async Task<bool> RefreshAuthAsync()
    {
        _messageWriter.WriteDebug("CETUS: Refreshing Auth");
        try
        {
            var result = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "api/v1/auth/token")
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Basic", _basicAuth) }
            }).ConfigureAwait(ConfigureAwaitOptions.None);
            _messageWriter.WriteDebug($"CETUS: Auth returned {(int)result.StatusCode}-{result.StatusCode}");
            if (!result.IsSuccessStatusCode) return false;
            if (result.StatusCode == HttpStatusCode.NoContent) return false;
            var authResult = await JsonSerializer.DeserializeAsync<AuthResult>(await result.Content.ReadAsStreamAsync().ConfigureAwait(ConfigureAwaitOptions.None)).ConfigureAwait(false);
            if (authResult?.AccessToken == null)
            {
                _messageWriter.WriteDebug("CETUS: Failed to parse token");
                return false;
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
            _tokenValid = DateTimeOffset.UtcNow.AddSeconds(authResult.ExpiresIn);
        }
        catch (Exception e)
        {
            _messageWriter.WriteDebug($"CETUS: Exception: {e}");
            return false;
        }
        return true;
    }

    public async Task<bool> AuthenticateAsync(Guid playerId, string apiKey)
    {
        _basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{playerId:N}:{apiKey}"));
        return await RefreshAuthAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    public async Task<SolvedResponse?> PostSolvedAsync(PlayerReport request)
    {
        await RefreshAuthIfNeeded().ConfigureAwait(ConfigureAwaitOptions.None);
        _messageWriter.WriteDebug("CETUS: Posting solved");
        return await MakePostAsync<SolvedResponse, PlayerReport>("api/v1/puzzle/solved", request).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<SeenResponse?> PostSeenAsync(PlayerReport request)
    {
        await RefreshAuthIfNeeded().ConfigureAwait(ConfigureAwaitOptions.None);
        _messageWriter.WriteDebug("CETUS: Posting seen");
        return await MakePostAsync<SeenResponse, PlayerReport>("api/v1/puzzle/seen", request).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<ScreenshotResponse?> PostScreenshotAsync(Screenshot screenshot)
    {
        await RefreshAuthIfNeeded().ConfigureAwait(ConfigureAwaitOptions.None);
        _messageWriter.WriteDebug("CETUS: Posting screenshot");
        return await MakePostAsync<ScreenshotResponse?, Screenshot>("api/v1/screenshots", screenshot).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<string?> GetSigninCodeAsync()
    {
        await RefreshAuthIfNeeded().ConfigureAwait(ConfigureAwaitOptions.None);
        _messageWriter.WriteDebug("CETUS: Fetching sign-in code");
        var reply = await MakeGetAsync<AuthResponse?>("api/v1/auth/webcode");
        return reply?.Code;
    }

    public async Task<PuzzleStatusResponse?> GetPuzzleStatusAsync(PuzzleStatusRequest request)
    {
        await RefreshAuthIfNeeded().ConfigureAwait(ConfigureAwaitOptions.None);
        _messageWriter.WriteDebug("CETUS: Requesting puzzle status");
        return await MakePostAsync<PuzzleStatusResponse, PuzzleStatusRequest>("api/v1/puzzle/status", request);
    }

    private async Task<TResponse?> MakePostAsync<TResponse, TRequest>(string requestUri, TRequest request, bool isRetry = false)
    {
        try
        {
            var result = await _httpClient.PostAsJsonAsync(requestUri, request)
                .ConfigureAwait(ConfigureAwaitOptions.None);
            _messageWriter.WriteDebug($"CETUS: Got a {(int)result.StatusCode}-{result.StatusCode}");
            if (!result.IsSuccessStatusCode)
            {
                if (!isRetry && result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _messageWriter.WriteDebug("CETUS: Retrying once with fresh auth");
                    var authResult = await RefreshAuthAsync().ConfigureAwait(ConfigureAwaitOptions.None);
                    if (!authResult) return default;
                    return await MakePostAsync<TResponse, TRequest>(requestUri, request, true).ConfigureAwait(ConfigureAwaitOptions.None);
                }
                return default;
            }
            return await result.Content.ReadFromJsonAsync<TResponse>().ConfigureAwait(ConfigureAwaitOptions.None);
        }
        catch (Exception e)
        {
            _messageWriter.WriteDebug($"CETUS: Exception: {e}");
            return default;
        }
    }


    public async Task<ZoneStatisticsResponse?> GetZoneStatisticsAsync(PuzzleZone zone)
    {
        await RefreshAuthIfNeeded().ConfigureAwait(ConfigureAwaitOptions.None);
        _messageWriter.WriteDebug($"CETUS: Requesting statistics for {zone}");
        return await MakeGetAsync<ZoneStatisticsResponse>($"api/v1.0/Puzzle/zone/{(int)zone}")
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<Sighting[]?> GetSightingsAsync(PuzzleZone zone, PuzzleType type)
    {
        await RefreshAuthIfNeeded().ConfigureAwait(ConfigureAwaitOptions.None);
        _messageWriter.WriteDebug($"CETUS: Requesting sightings for {zone} {type}");
        return await MakeGetAsync<Sighting[]>($"api/v1.0/Puzzle/sightings/{(int)zone}/{(int)type}")
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<ZoneUnsolvedResponse?> GetSightedUnsolved(PuzzleZone zone)
    {
        await RefreshAuthIfNeeded().ConfigureAwait(ConfigureAwaitOptions.None);
        _messageWriter.WriteDebug($"CETUS: Requesting unsolved sightings for {zone}");
        return await MakeGetAsync<ZoneUnsolvedResponse>($"api/v1/zone/{(int)zone}/unsolved/")
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async Task<T?> MakeGetAsync<T>(string requestUri, bool isRetry = false)
    {
        try
        {
            var result = await _httpClient.GetAsync(requestUri).ConfigureAwait(ConfigureAwaitOptions.None);
            _messageWriter.WriteDebug($"CETUS: Got a {(int)result.StatusCode}-{result.StatusCode}");
            if (!result.IsSuccessStatusCode)
            {
                if (!isRetry && result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _messageWriter.WriteDebug("CETUS: Retrying once with fresh auth");
                    var authResult = await RefreshAuthAsync().ConfigureAwait(ConfigureAwaitOptions.None);
                    if (!authResult) return default;
                    return await MakeGetAsync<T>(requestUri, true).ConfigureAwait(ConfigureAwaitOptions.None);
                }
                return default;
            }
            return await result.Content.ReadFromJsonAsync<T>().ConfigureAwait(ConfigureAwaitOptions.None);
        }
        catch (Exception e)
        {
            _messageWriter.WriteDebug($"CETUS: Exception: {e}");
            return default;
        }
    }
}