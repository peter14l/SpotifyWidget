using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using SpotifyWidget.Models;
using SpotifyWidget.Services;

namespace SpotifyWidget.Controls;

public sealed partial class SpotifyPlayerControl : UserControl
{
    private ISpotifyService? _spotifyService;
    private SpotifyPlaybackState? _currentState;

    public SpotifyPlayerControl()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        _spotifyService = App.GetService<ISpotifyService>();
        if (_spotifyService != null)
        {
            _spotifyService.PlaybackStateChanged += OnPlaybackStateChanged;
            await RefreshState();
        }
    }

    private async Task RefreshState()
    {
        if (_spotifyService == null) return;

        var state = await _spotifyService.GetPlaybackStateAsync();
        UpdateUI(state);
    }

    private void OnPlaybackStateChanged(object? sender, SpotifyPlaybackState? state)
    {
        DispatcherQueue.TryEnqueue(() => UpdateUI(state));
    }

    private void UpdateUI(SpotifyPlaybackState? state)
    {
        _currentState = state;

        if (state?.Track == null)
        {
            TrackNameText.Text = "No track playing";
            ArtistNameText.Text = "";
            AlbumArtImage.Source = null;
            PlayPauseButton.Content = "\uE768";
            ProgressBar.Value = 0;
            ProgressBar.Maximum = 1;
            return;
        }

        TrackNameText.Text = state.Track.Name;
        ArtistNameText.Text = state.Track.ArtistNames;

        if (state.Track.AlbumArtUri != null)
        {
            AlbumArtImage.Source = new BitmapImage(state.Track.AlbumArtUri);
        }

        PlayPauseButton.Content = state.IsPlaying ? "\uE769" : "\uE768";

        if (state.Track.DurationMs > 0)
        {
            ProgressBar.Maximum = state.Track.DurationMs;
            ProgressBar.Value = state.ProgressMs;
        }

        UpdateTimeDisplay(state.ProgressMs, state.Track.DurationMs);
    }

    private void UpdateTimeDisplay(int currentMs, int totalMs)
    {
        var current = TimeSpan.FromMilliseconds(currentMs);
        var total = TimeSpan.FromMilliseconds(totalMs);
        TimeText.Text = $"{current:mm\\:ss} / {total:mm\\:ss}";
    }

    private async void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        if (_spotifyService == null || _currentState == null) return;

        if (_currentState.IsPlaying)
            await _spotifyService.PauseAsync();
        else
            await _spotifyService.PlayAsync();
    }

    private async void NextButton_Click(object sender, RoutedEventArgs e)
    {
        if (_spotifyService != null)
            await _spotifyService.NextTrackAsync();
    }

    private async void PreviousButton_Click(object sender, RoutedEventArgs e)
    {
        if (_spotifyService != null)
            await _spotifyService.PreviousTrackAsync();
    }

    private async void ProgressBar_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (_spotifyService == null || _currentState?.Track == null) return;

        if (Math.Abs(e.NewValue - e.OldValue) > 1000)
        {
            await _spotifyService.SeekAsync((int)e.NewValue);
        }
    }
}
