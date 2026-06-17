using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;
using SpotifyWidget.Models;
using SpotifyWidget.Services;

namespace SpotifyWidget.ViewModels;

public partial class SpotifyPlayerViewModel : ObservableObject, IDisposable
{
    private readonly ISpotifyService _spotifyService;
    private readonly ISettingsService _settingsService;
    private readonly DispatcherQueue _dispatcherQueue;

    [ObservableProperty]
    private string _trackName = "No track playing";

    [ObservableProperty]
    private string _artistName = string.Empty;

    [ObservableProperty]
    private BitmapImage? _albumArt;

    [ObservableProperty]
    private string _playPauseSymbol = "\uE768";

    [ObservableProperty]
    private double _progressValue;

    [ObservableProperty]
    private double _progressMaximum = 1;

    [ObservableProperty]
    private string _timeDisplay = "0:00 / 0:00";

    [ObservableProperty]
    private bool _isPlaying;

    [ObservableProperty]
    private string _statusText = "Not authenticated";

    public SpotifyPlayerViewModel(ISpotifyService spotifyService, ISettingsService settingsService)
    {
        _spotifyService = spotifyService;
        _settingsService = settingsService;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        _spotifyService.PlaybackStateChanged += OnPlaybackStateChanged;
        _spotifyService.AuthenticationStateChanged += OnAuthenticationStateChanged;
    }

    public async Task InitializeAsync()
    {
        var isAuthenticated = await _spotifyService.IsAuthenticatedAsync();
        if (isAuthenticated)
        {
            await RefreshStateAsync();
            _spotifyService.StartPolling(_settingsService.GetSettings().RefreshIntervalMs);
            StatusText = "Connected";
        }
        else
        {
            StatusText = "Not authenticated";
        }
    }

    private void OnPlaybackStateChanged(object? sender, SpotifyPlaybackState? state)
    {
        _dispatcherQueue.TryEnqueue(() => UpdateUI(state));
    }

    private void OnAuthenticationStateChanged(object? sender, bool authenticated)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            StatusText = authenticated ? "Connected" : "Not authenticated";
        });
    }

    private async Task RefreshStateAsync()
    {
        var state = await _spotifyService.GetPlaybackStateAsync();
        UpdateUI(state);
    }

    private void UpdateUI(SpotifyPlaybackState? state)
    {
        if (state?.Track == null)
        {
            TrackName = state?.Device != null ? "No track playing" : "No track playing";
            ArtistName = string.Empty;
            AlbumArt = null;
            PlayPauseSymbol = "\uE768";
            ProgressValue = 0;
            ProgressMaximum = 1;
            TimeDisplay = "0:00 / 0:00";
            IsPlaying = false;
            return;
        }

        TrackName = state.Track.Name;
        ArtistName = state.Track.ArtistNames;
        IsPlaying = state.IsPlaying;
        PlayPauseSymbol = state.IsPlaying ? "\uE769" : "\uE768";

        if (state.Track.AlbumArtUri != null)
        {
            AlbumArt = new BitmapImage(state.Track.AlbumArtUri);
        }

        if (state.Track.DurationMs > 0)
        {
            ProgressMaximum = state.Track.DurationMs;
            ProgressValue = state.ProgressMs;
        }

        UpdateTimeDisplay(state.ProgressMs, state.Track.DurationMs);
    }

    private void UpdateTimeDisplay(int currentMs, int totalMs)
    {
        var current = TimeSpan.FromMilliseconds(currentMs);
        var total = TimeSpan.FromMilliseconds(totalMs);
        TimeDisplay = $"{current:mm\\:ss} / {total:mm\\:ss}";
    }

    [RelayCommand]
    private async Task PlayPauseAsync()
    {
        if (IsPlaying)
            await _spotifyService.PauseAsync();
        else
            await _spotifyService.PlayAsync();
    }

    [RelayCommand]
    private async Task NextTrackAsync()
    {
        await _spotifyService.NextTrackAsync();
    }

    [RelayCommand]
    private async Task PreviousTrackAsync()
    {
        await _spotifyService.PreviousTrackAsync();
    }

    [RelayCommand]
    private async Task SeekAsync(double position)
    {
        if (Math.Abs(position - ProgressValue) > 1000)
        {
            await _spotifyService.SeekAsync((int)position);
        }
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        await _spotifyService.StartAuthFlowAsync();
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _spotifyService.LogoutAsync();
    }

    public void Dispose()
    {
        _spotifyService.PlaybackStateChanged -= OnPlaybackStateChanged;
        _spotifyService.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        _spotifyService.StopPolling();
    }
}