using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SpotifyWidget.Models;

namespace SpotifyWidget.Services;

public class SpotifyService : ISpotifyService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settingsService;
    private Timer? _pollTimer;
    private HttpListener? _callbackListener;

    private string _accessToken = string.Empty;
    private string _refreshToken = string.Empty;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private string _codeVerifier = string.Empty;

    private const string ClientId = "YOUR_SPOTIFY_CLIENT_ID";
    private const string RedirectUri = "http://localhost:52763/callback";
    private const string Scopes = "user-read-playback-state user-modify-playback-state user-read-currently-playing user-read-private";
    private const string TokenUrl = "https://accounts.spotify.com/api/token";
    private const string AuthUrl = "https://accounts.spotify.com/authorize";
    private const string ApiBase = "https://api.spotify.com/v1";

    public event EventHandler<SpotifyPlaybackState?>? PlaybackStateChanged;
    public event EventHandler<bool>? AuthenticationStateChanged;

    public SpotifyService(ISettingsService settingsService, HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _settingsService = settingsService;
    }

    public void Initialize()
    {
        var storedRefreshToken = _settingsService.GetSettings().RefreshToken;
        if (!string.IsNullOrEmpty(storedRefreshToken))
        {
            _refreshToken = storedRefreshToken;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
            return true;

        if (!string.IsNullOrEmpty(_refreshToken))
        {
            return await RefreshAccessTokenAsync();
        }

        return false;
    }

    public async Task<string> GetAuthorizationUrlAsync()
    {
        _codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(_codeVerifier);
        var state = GenerateRandomState();

        var queryParams = new Dictionary<string, string>
        {
            ["client_id"] = ClientId,
            ["response_type"] = "code",
            ["redirect_uri"] = RedirectUri,
            ["code_challenge_method"] = "S256",
            ["code_challenge"] = codeChallenge,
            ["scope"] = Scopes,
            ["state"] = state
        };

        var queryString = string.Join("&", queryParams.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return $"{AuthUrl}?{queryString}";
    }

    public async Task<bool> HandleCallbackAsync(string authorizationCode)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, TokenUrl);
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", authorizationCode),
                new KeyValuePair<string, string>("redirect_uri", RedirectUri),
                new KeyValuePair<string, string>("code_verifier", _codeVerifier)
            });
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return false;

            var tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(json);
            if (tokenResponse == null)
                return false;

            ApplyTokenResponse(tokenResponse);
            await PersistRefreshTokenAsync();

            AuthenticationStateChanged?.Invoke(this, true);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during callback: {ex.Message}");
            return false;
        }
    }

    public async Task StartAuthFlowAsync()
    {
        var authUrl = await GetAuthorizationUrlAsync();
        await StartCallbackListenerAsync();
        await Windows.System.Launcher.LaunchUriAsync(new Uri(authUrl));
    }

    private async Task StartCallbackListenerAsync()
    {
        if (_callbackListener != null)
        {
            _callbackListener.Stop();
            _callbackListener.Close();
        }

        _callbackListener = new HttpListener();
        _callbackListener.Prefixes.Add("http://localhost:52763/");
        _callbackListener.Start();

        _ = Task.Run(async () =>
        {
            try
            {
                var context = await _callbackListener.GetContextAsync();
                var query = context.Request.QueryString;
                var code = query["code"];
                var state = query["state"];

                var response = context.Response;
                if (!string.IsNullOrEmpty(code))
                {
                    await HandleCallbackAsync(code);
                    var successHtml = "<html><body><h1>Authentication successful!</h1><p>You can close this window.</p></body></html>";
                    var buffer = Encoding.UTF8.GetBytes(successHtml);
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer);
                }
                else
                {
                    var errorHtml = "<html><body><h1>Authentication failed.</h1><p>Please try again.</p></body></html>";
                    var buffer = Encoding.UTF8.GetBytes(errorHtml);
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer);
                }
                response.OutputStream.Close();
            }
            catch (ObjectDisposedException) { }
            catch (HttpListenerException) { }
        });
    }

    private async Task<bool> RefreshAccessTokenAsync()
    {
        if (string.IsNullOrEmpty(_refreshToken))
            return false;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, TokenUrl);
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", _refreshToken)
            });
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _refreshToken = string.Empty;
                await PersistRefreshTokenAsync();
                AuthenticationStateChanged?.Invoke(this, false);
                return false;
            }

            var tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(json);
            if (tokenResponse == null)
                return false;

            ApplyTokenResponse(tokenResponse);
            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                _refreshToken = tokenResponse.RefreshToken;
                await PersistRefreshTokenAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing token: {ex.Message}");
            return false;
        }
    }

    private void ApplyTokenResponse(SpotifyTokenResponse tokenResponse)
    {
        _accessToken = tokenResponse.AccessToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);

        if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
            _refreshToken = tokenResponse.RefreshToken;
    }

    private async Task PersistRefreshTokenAsync()
    {
        var settings = _settingsService.GetSettings();
        settings.RefreshToken = _refreshToken;
        await _settingsService.SaveSettingsAsync(settings);
    }

    public async Task<SpotifyPlaybackState?> GetPlaybackStateAsync()
    {
        if (!await IsAuthenticatedAsync())
            return null;

        try
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"{ApiBase}/me/player");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await RefreshAccessTokenAsync())
                {
                    SetAuthHeader();
                    response = await _httpClient.GetAsync($"{ApiBase}/me/player");
                }
                else
                {
                    return null;
                }
            }

            if (response.StatusCode == HttpStatusCode.NoContent)
                return null;

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<SpotifyPlaybackState>(json);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting playback state: {ex.Message}");
        }

        return null;
    }

    public async Task<bool> PlayAsync()
    {
        return await SendPlaybackCommand(HttpMethod.Put, $"{ApiBase}/me/player/play");
    }

    public async Task<bool> PauseAsync()
    {
        return await SendPlaybackCommand(HttpMethod.Put, $"{ApiBase}/me/player/pause");
    }

    public async Task<bool> NextTrackAsync()
    {
        return await SendPlaybackCommand(HttpMethod.Post, $"{ApiBase}/me/player/next");
    }

    public async Task<bool> PreviousTrackAsync()
    {
        return await SendPlaybackCommand(HttpMethod.Post, $"{ApiBase}/me/player/previous");
    }

    public async Task<bool> SetVolumeAsync(int volume)
    {
        volume = Math.Clamp(volume, 0, 100);
        return await SendPlaybackCommand(HttpMethod.Put, $"{ApiBase}/me/player/volume?volume_percent={volume}");
    }

    public async Task<bool> SeekAsync(int positionMs)
    {
        return await SendPlaybackCommand(HttpMethod.Put, $"{ApiBase}/me/player/seek?position_ms={positionMs}");
    }

    public async Task<List<SpotifyDevice>> GetDevicesAsync()
    {
        var result = new List<SpotifyDevice>();
        if (!await IsAuthenticatedAsync())
            return result;

        try
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"{ApiBase}/me/player/devices");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var deviceResponse = JsonSerializer.Deserialize<SpotifyDevicesResponse>(json);
                if (deviceResponse?.Devices != null)
                    result = deviceResponse.Devices;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting devices: {ex.Message}");
        }

        return result;
    }

    public async Task<bool> TransferPlaybackAsync(string deviceId)
    {
        if (!await IsAuthenticatedAsync())
            return false;

        try
        {
            SetAuthHeader();
            var payload = JsonSerializer.Serialize(new { device_ids = new[] { deviceId } });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{ApiBase}/me/player", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error transferring playback: {ex.Message}");
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        _accessToken = string.Empty;
        _refreshToken = string.Empty;
        _tokenExpiry = DateTime.MinValue;

        var settings = _settingsService.GetSettings();
        settings.RefreshToken = string.Empty;
        await _settingsService.SaveSettingsAsync(settings);

        StopPolling();
        AuthenticationStateChanged?.Invoke(this, false);
    }

    private async Task<bool> SendPlaybackCommand(HttpMethod method, string url)
    {
        if (!await IsAuthenticatedAsync())
            return false;

        try
        {
            SetAuthHeader();
            var request = new HttpRequestMessage(method, url);
            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await RefreshAccessTokenAsync())
                {
                    SetAuthHeader();
                    request = new HttpRequestMessage(method, url);
                    response = await _httpClient.SendAsync(request);
                    return response.IsSuccessStatusCode;
                }
                return false;
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error sending command: {ex.Message}");
            return false;
        }
    }

    private void SetAuthHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    public void StartPolling(int intervalMs)
    {
        _pollTimer?.Dispose();
        _pollTimer = new Timer(async _ => await PollPlaybackState(), null, 0, intervalMs);
    }

    public void StopPolling()
    {
        _pollTimer?.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private async Task PollPlaybackState()
    {
        try
        {
            var state = await GetPlaybackStateAsync();
            PlaybackStateChanged?.Invoke(this, state);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Polling error: {ex.Message}");
        }
    }

    private static string GenerateCodeVerifier()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        return Convert.ToBase64String(challengeBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string GenerateRandomState()
    {
        var randomBytes = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToHexString(randomBytes).ToLower();
    }

    public void Dispose()
    {
        _pollTimer?.Dispose();
        _httpClient?.Dispose();
        _callbackListener?.Stop();
        _callbackListener?.Close();
        GC.SuppressFinalize(this);
    }
}