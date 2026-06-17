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
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (args.NewValue is SpotifyPlayerViewModel viewModel)
        {
            ViewModel = viewModel;
            Bindings.Update();
            _ = viewModel.InitializeAsync();
        }
    }
}