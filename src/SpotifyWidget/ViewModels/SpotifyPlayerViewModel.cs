using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;
using SpotifyWidget.Models;
using SpotifyWidget.Services;
using Windows.UI;
using System.IO;

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

    [ObservableProperty]
    private bool _isAuthenticated;

    [ObservableProperty]
    private Color _accentColor = Color.FromArgb(0, 0, 0, 0);

    private string? _lastAlbumArtUri;
    private bool _initialized;

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
        if (_initialized)
            return;
        _initialized = true;

        await _settingsService.LoadSettingsAsync();
        _spotifyService.Initialize();
        var isAuthenticated = await _spotifyService.IsAuthenticatedAsync();
        IsAuthenticated = isAuthenticated;
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
        _dispatcherQueue.TryEnqueue(async () =>
        {
            IsAuthenticated = authenticated;
            if (authenticated)
            {
                StatusText = "Connected";
                await RefreshStateAsync();
                _spotifyService.StartPolling(_settingsService.GetSettings().RefreshIntervalMs);
            }
            else
            {
                StatusText = "Not authenticated";
            }
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

        var albumArtUri = state.Track.AlbumArtUri;
        if (albumArtUri != null)
        {
            var uriStr = albumArtUri.ToString();
            if (!string.Equals(uriStr, _lastAlbumArtUri))
            {
                _lastAlbumArtUri = uriStr;
                AlbumArt = new BitmapImage(albumArtUri);
                _ = ExtractColorsAsync(uriStr);
            }
        }
        else
        {
            _lastAlbumArtUri = null;
            AlbumArt = null;
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

    private async Task ExtractColorsAsync(string imageUri)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(3);
            var bytes = await httpClient.GetByteArrayAsync(imageUri);

            using var stream = new MemoryStream(bytes).AsRandomAccessStream();
            var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
            var pixelData = await decoder.GetPixelDataAsync(
                Windows.Graphics.Imaging.BitmapPixelFormat.Rgba8,
                Windows.Graphics.Imaging.BitmapAlphaMode.Ignore,
                new Windows.Graphics.Imaging.BitmapTransform(),
                Windows.Graphics.Imaging.ExifOrientationMode.IgnoreExifOrientation,
                Windows.Graphics.Imaging.ColorManagementMode.DoNotColorManage);

            var pixels = pixelData.DetachPixelData();
            var dominant = GetDominantColor(pixels, (int)decoder.PixelWidth, (int)decoder.PixelHeight);

            _dispatcherQueue.TryEnqueue(() => AccentColor = dominant);
        }
        catch
        {
        }
    }

    private static Color GetDominantColor(byte[] pixels, int width, int height)
    {
        var step = Math.Max(1, width * height / 200);
        long totalR = 0, totalG = 0, totalB = 0;
        var count = 0;

        for (var i = 0; i < pixels.Length - 3; i += 4 * step)
        {
            totalB += pixels[i];
            totalG += pixels[i + 1];
            totalR += pixels[i + 2];
            count++;
        }

        if (count == 0)
            return Color.FromArgb(0, 0, 0, 0);

        return Color.FromArgb(
            100,
            (byte)(totalR / count),
            (byte)(totalG / count),
            (byte)(totalB / count));
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