using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SpotifyWidget.ViewModels;

namespace SpotifyWidget.Controls;

public sealed partial class SpotifyPlayerControl : UserControl
{
    public SpotifyPlayerViewModel? ViewModel { get; private set; }

    public SpotifyPlayerControl()
    {
        this.InitializeComponent();

        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateResponsiveLayout();
        UpdateProgressBarWidth();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateResponsiveLayout();
        UpdateProgressBarWidth();
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (args.NewValue is SpotifyPlayerViewModel viewModel)
        {
            ViewModel = viewModel;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            UpdateProgressBarWidth();
            UpdateAuthVisibility();
            Bindings.Update();
            _ = viewModel.InitializeAsync();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SpotifyPlayerViewModel.ProgressValue) ||
            e.PropertyName == nameof(SpotifyPlayerViewModel.ProgressMaximum))
        {
            UpdateProgressBarWidth();
        }
        else if (e.PropertyName == nameof(SpotifyPlayerViewModel.IsAuthenticated))
        {
            UpdateAuthVisibility();
        }
    }

    private void UpdateAuthVisibility()
    {
        if (ViewModel is null)
            return;

        var isAuth = ViewModel.IsAuthenticated;
        PlayerContent.Visibility = isAuth ? Visibility.Visible : Visibility.Collapsed;
        LoginContent.Visibility = isAuth ? Visibility.Collapsed : Visibility.Visible;
    }

    private bool _isSeeking;

    private double _artSize = 80;

    private void UpdateResponsiveLayout()
    {
        var availableWidth = ActualWidth - 16;
        _artSize = Math.Clamp(availableWidth * 0.28, 60.0, 200.0);
        AlbumArtBorder.Width = _artSize;
        AlbumArtBorder.Height = _artSize;

        var scale = _artSize / 80.0;
        TrackNameText.FontSize = Math.Clamp(14.0 * scale, 12.0, 24.0);
        ArtistNameText.FontSize = Math.Clamp(12.0 * scale, 10.0, 18.0);
        ArtistNameText.Margin = new Thickness(0, Math.Clamp(4 * scale, 2, 8), 0, 0);

        var btnSize = Math.Clamp(32 * scale, 28.0, 48.0);
        PrevButton.Width = btnSize;
        PrevButton.Height = btnSize;
        NextButton.Width = btnSize;
        NextButton.Height = btnSize;

        var playSize = Math.Clamp(48 * scale, 40.0, 72.0);
        PlayPauseButton.Width = playSize;
        PlayPauseButton.Height = playSize;

        var btnFont = Math.Clamp(16 * scale, 14.0, 24.0);
        PrevButton.FontSize = btnFont;
        NextButton.FontSize = btnFont;
        PlayPauseButton.FontSize = Math.Clamp(20 * scale, 18.0, 30.0);

        ProgressTrack.Height = Math.Clamp(20 * scale, 16.0, 40.0);
    }

    private void UpdateProgressBarWidth()
    {
        if (_isSeeking)
            return;

        var vm = ViewModel;
        if (vm is null)
            return;

        var parentWidth = ActualWidth - 16;
        if (parentWidth <= 0 || vm.ProgressMaximum <= 0)
        {
            ProgressFill.Width = 0;
            return;
        }

        ProgressFill.Width = parentWidth * (vm.ProgressValue / vm.ProgressMaximum);
    }

    private double GetProgressFromPointerX(double pointerX)
    {
        var parentWidth = ActualWidth - 16;
        if (parentWidth <= 0)
            return 0;

        var ratio = Math.Clamp((pointerX - 8) / parentWidth, 0, 1);
        return ratio * (ViewModel?.ProgressMaximum ?? 1);
    }

    private void OnProgressPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _isSeeking = true;
        var point = e.GetCurrentPoint(ProgressTrack);
        var positionMs = GetProgressFromPointerX(point.Position.X);
        ProgressFill.Width = Math.Max(0, point.Position.X - 8);
        ViewModel?.SeekCommand?.Execute(positionMs);
    }

    private void OnProgressPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isSeeking)
            return;

        var point = e.GetCurrentPoint(ProgressTrack);
        var positionMs = GetProgressFromPointerX(point.Position.X);
        ProgressFill.Width = Math.Max(0, point.Position.X - 8);
    }

    private void OnProgressPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (!_isSeeking)
            return;

        var point = e.GetCurrentPoint(ProgressTrack);
        var positionMs = GetProgressFromPointerX(point.Position.X);
        ViewModel?.SeekCommand?.Execute(positionMs);
        _isSeeking = false;
        UpdateProgressBarWidth();
    }

    private void OnProgressPointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        _isSeeking = false;
        UpdateProgressBarWidth();
    }

    private void OnProgressPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (_isSeeking)
        {
            _isSeeking = false;
            UpdateProgressBarWidth();
        }
    }
}
