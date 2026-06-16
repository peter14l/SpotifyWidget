using SpotifyWidget.Models;

namespace SpotifyWidget.Services;

public interface ISpotifyService
{
    Task<bool> IsAuthenticatedAsync();
    Task<SpotifyPlaybackState?> GetPlaybackStateAsync();
    Task<bool> PlayAsync();
    Task<bool> PauseAsync();
    Task<bool> NextTrackAsync();
    Task<bool> PreviousTrackAsync();
    Task<bool> SetVolumeAsync(int volume);
    Task<bool> SeekAsync(int positionMs);
    event EventHandler<SpotifyPlaybackState?>? PlaybackStateChanged;
    event EventHandler<bool>? AuthenticationStateChanged;
}
