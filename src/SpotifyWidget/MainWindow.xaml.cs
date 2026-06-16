using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SpotifyWidget.Services;
using Windows.Foundation;
using WinRT.Interop;

namespace SpotifyWidget;

public sealed partial class MainWindow : Window
{
    private AppWindow _appWindow = null!;
    private readonly IThemeService _themeService;

    public MainWindow()
    {
        this.InitializeComponent();
        _themeService = App.GetService<IThemeService>()!;

        InitializeWindow();
        InitializeMicaBackdrop();
        ApplyTheme();
    }

    private void InitializeWindow()
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        _appWindow = AppWindow.GetFromWindowId(windowId);

        _appWindow.Title = "Spotify Widget";
        _appWindow.IsShownInSwitchers = false;

        var presenter = _appWindow.Presenter as OverlappedPresenter;
        if (presenter != null)
        {
            presenter.IsAlwaysOnTop = true;
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
            presenter.IsResizable = true;
            presenter.PreferredMinimumWidth = 320;
            presenter.PreferredMinimumHeight = 200;
            presenter.PreferredMaximumWidth = 480;
            presenter.PreferredMaximumHeight = 400;
        }

        _appWindow.Changed += OnWindowChanged;
    }

    private void OnWindowChanged(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (args.DidZOrderChange)
        {
            sender.MoveInZOrderAtBottom();
        }

        if (args.DidPositionChange)
        {
            _themeService.SaveWindowPosition(new Point(sender.Position.X, sender.Position.Y));
        }
    }

    private void InitializeMicaBackdrop()
    {
        TrySetMicaBackdrop();
    }

    private bool TrySetMicaBackdrop()
    {
        if (MicaController.IsSupported())
        {
            var micaController = new MicaController();
            micaController.Kind = MicaKind.Base;

            var backdropConfiguration = new SystemBackdropConfiguration();
            backdropConfiguration.IsInputActive = true;
            backdropConfiguration.TintColor = _themeService.GetTintColor();
            backdropConfiguration.TintOpacity = 0.8;
            backdropConfiguration.LuminosityOpacity = 0.4;

            micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
            micaController.SetSystemBackdropConfiguration(backdropConfiguration);

            return true;
        }

        return false;
    }

    private void ApplyTheme()
    {
        var theme = _themeService.GetCurrentTheme();
        if (Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = theme == ElementTheme.Dark ? ElementTheme.Dark : ElementTheme.Light;
        }
    }
}
