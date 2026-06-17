using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SpotifyWidget.Services;
using SpotifyWidget.ViewModels;
using Windows.Foundation;
using WinRT;
using WinRT.Interop;

namespace SpotifyWidget;

public sealed partial class MainWindow : Window
{
    public SpotifyPlayerViewModel ViewModel { get; }

    private AppWindow _appWindow = null!;
    private readonly IThemeService _themeService;
    private readonly ISettingsService _settingsService;
    private MicaController? _micaController;
    private SystemBackdropConfiguration? _backdropConfiguration;

    public MainWindow(IThemeService themeService, SpotifyPlayerViewModel viewModel, ISettingsService settingsService)
    {
        this.InitializeComponent();
        ViewModel = viewModel;
        _themeService = themeService;
        _settingsService = settingsService;
        RootGrid.DataContext = viewModel;

        InitializeWindow();
        InitializeMicaBackdrop();
        ApplyTheme();
        RestoreWindowPosition();
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
            var settings = _settingsService.GetSettings();
            presenter.IsAlwaysOnTop = settings.AlwaysOnTop;
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
            presenter.IsResizable = true;
            presenter.PreferredMinimumWidth = 320;
            presenter.PreferredMinimumHeight = 200;
            presenter.PreferredMaximumWidth = 480;
            presenter.PreferredMaximumHeight = 400;
        }

        _appWindow.Changed += OnWindowChanged;
        _appWindow.Destroying += OnWindowDestroying;
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

    private void RestoreWindowPosition()
    {
        var position = _themeService.LoadWindowPosition();
        if (position.X >= 0 && position.Y >= 0)
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            SetWindowPos(hwnd, IntPtr.Zero, (int)position.X, (int)position.Y, 0, 0, 0x0001 | 0x0004);
        }
    }

    private void InitializeMicaBackdrop()
    {
        TrySetMicaBackdrop();
    }

    private bool TrySetMicaBackdrop()
    {
        try
        {
            if (MicaController.IsSupported())
            {
                _micaController = new MicaController();
                _micaController.Kind = MicaKind.Base;

                _backdropConfiguration = new SystemBackdropConfiguration
                {
                    IsInputActive = true,
                    Theme = _themeService.GetCurrentTheme() switch
                    {
                        ElementTheme.Dark => SystemBackdropTheme.Dark,
                        ElementTheme.Light => SystemBackdropTheme.Light,
                        _ => SystemBackdropTheme.Default
                    }
                };

                var tint = _themeService.GetTintColor();
                _micaController.TintColor = tint;
                _micaController.TintOpacity = 0.4f;
                _micaController.LuminosityOpacity = 0.8f;

                _micaController.SetSystemBackdropConfiguration(_backdropConfiguration);
                _micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());

                return true;
            }
        }
        catch
        {
            System.Diagnostics.Debug.WriteLine("Mica backdrop not supported, continuing without it");
        }

        return false;
    }

    private void ApplyTheme()
    {
        var theme = _themeService.GetCurrentTheme();
        if (Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = theme;
        }
    }

    private void SettingsButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var settingsWindow = App.Services.GetRequiredService<Controls.SettingsWindow>();
        settingsWindow.Activate();
    }

    private void OnWindowDestroying(AppWindow sender, object args)
    {
        ViewModel.Dispose();
        ((App)Microsoft.UI.Xaml.Application.Current).DisposeHost();
    }

    private void CloseButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        this.Close();
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
}