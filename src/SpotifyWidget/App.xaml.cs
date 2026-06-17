using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using SpotifyWidget.Services;
using SpotifyWidget.ViewModels;
using WinRT.Interop;

namespace SpotifyWidget;

public partial class App : Application
{
    private Window? _window;
    private ServiceProvider? _serviceProvider;

    public static IServiceProvider Services { get; private set; } = null!;
    public App()
    {
        this.InitializeComponent();
        ConfigureServices();
    }

    private void ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<ISpotifyService, SpotifyService>();
        services.AddSingleton<MainWindow>();
        services.AddTransient<Controls.SettingsWindow>();
        services.AddSingleton<SpotifyPlayerViewModel>();
        _serviceProvider = services.BuildServiceProvider();

        Services = _serviceProvider;
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = Services.GetRequiredService<MainWindow>();
        _window.Activate();

        var hwnd = WindowNative.GetWindowHandle(_window);
        SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
    }

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_SHOWWINDOW = 0x0040;

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    public void DisposeHost()
    {
        _serviceProvider?.Dispose();
    }
}