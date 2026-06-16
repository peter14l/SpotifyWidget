using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SpotifyWidget.Models;

namespace SpotifyWidget.Services;

public class SpotifyService : ISpotifyService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settingsService;
    private readonly Timer _pollTimer;

    private string _accessToken = string.Empty;
    private string _refreshToken = string.Empty;
    private DateTime _tokenExpiry = DateTime.MinValue;

    private const string ClientId = "YOUR_SPOTIFY_CLIENT_ID";
    private const string RedirectUri = "http://localhost:52763/callback";
    private const string Scopes = "user-read-playback-state user-modify-playback-state user-read-currently-playing";

    public event EventHandler<SpotifyPlaybackState?>? PlaybackStateChanged;
    public event EventHandler<bool>? AuthenticationStateChanged;

    public SpotifyService(ISettingsService settingsService)
    {
        _httpClient = new HttpClient();
        _settingsService = settingsService;
        _pollTimer = new Timer(async _ => await PollPlaybackState(), null, Timeout.Infinite, Timeout.Infinite);
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry);
    }

    public async Task<SpotifyPlaybackState?> GetPlaybackStateAsync()
    {
        if (!await IsAuthenticatedAsync())
            return null;

        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            var response = await _httpClient.GetAsync("https://api.spotify.com/v1/me/player");

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
        return await SendPlaybackCommand("PUT", "https://api.spotify.com/v1/me/player/play");
    }

    public async Task<bool> PauseAsync()
    {
        return await SendPlaybackCommand("PUT", "https://api.spotify.com/v1/me/player/pause");
    }

    public async Task<bool> NextTrackAsync()
    {
        return await SendPlaybackCommand("POST", "https://api.spotify.com/v1/me/player/next");
    }

    public async Task<bool> PreviousTrackAsync()
    {
        return await SendPlaybackCommand("POST", "https://api.spotify.com/v1/me/player/previous");
    }

    public async Task<bool> SetVolumeAsync(int volume)
    {
        volume = Math.Clamp(volume, 0, 100);
        return await SendPlaybackCommand("PUT", $"https://api.spotify.com/v1/me/player/volume?volume_percent={volume}");
    }

    public async Task<bool> SeekAsync(int positionMs)
    {
        return await SendPlaybackCommand("PUT", $"https://api.spotify.com/v1/me/player/seek?position_ms={positionMs}");
    }

    private async Task<bool> SendPlaybackCommand(string method, string url)
    {
        if (!await IsAuthenticatedAsync())
            return false;

        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            var request = new HttpRequestMessage(new HttpMethod(method), url);
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error sending command: {ex.Message}");
            return false;
        }
    }

    private async Task PollPlaybackState()
    {
        var state = await GetPlaybackStateAsync();
        PlaybackStateChanged?.Invoke(this, state);
    }

    public void StartPolling(int intervalMs)
    {
        _pollTimer.Change(0, intervalMs);
    }

    public void StopPolling()
    {
        _pollTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    public void Dispose()
    {
        _pollTimer?.Dispose();
        _httpClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}
