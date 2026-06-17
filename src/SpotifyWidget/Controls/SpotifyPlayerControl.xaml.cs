using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
        UpdateProgressBarWidth();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
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

    private void UpdateProgressBarWidth()
    {
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
}
