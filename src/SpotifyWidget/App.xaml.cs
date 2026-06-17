using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using SpotifyWidget.Services;
using SpotifyWidget.ViewModels;

namespace SpotifyWidget;

public partial class App : Application
{
    private Window? _window;
    private ServiceProvider? _serviceProvider;

    public static IServiceProvider Services { get; private set; } = null!;
    private static readonly string LogPath = @"C:\Users\LOQ\AppData\Local\Temp\app_trace.log";

    public App()
    {
        try
        {
            System.IO.File.AppendAllText(LogPath, "=== App ctor start ===\r\n");
            this.InitializeComponent();
            System.IO.File.AppendAllText(LogPath, "After InitComponent\r\n");
            ConfigureServices();
            System.IO.File.AppendAllText(LogPath, "App ctor done\r\n");
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText(LogPath, $"App ctor failed: {ex}\r\n");
            throw;
        }
    }

    private void ConfigureServices()
    {
        System.IO.File.AppendAllText(LogPath, "ConfigServices start\r\n");
        var services = new ServiceCollection();
        System.IO.File.AppendAllText(LogPath, "ServiceCollection created\r\n");
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<ISpotifyService, SpotifyService>();
        services.AddSingleton<MainWindow>();
        services.AddTransient<Controls.SettingsWindow>();
        services.AddSingleton<SpotifyPlayerViewModel>();
        System.IO.File.AppendAllText(LogPath, "Services registered\r\n");
        _serviceProvider = services.BuildServiceProvider();
        System.IO.File.AppendAllText(LogPath, "After Build\r\n");

        Services = _serviceProvider;
        System.IO.File.AppendAllText(LogPath, "ConfigServices done\r\n");
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            System.IO.File.AppendAllText(LogPath, $"OnLaunched start (Services null={Services is null})\r\n");
            if (Services is null)
            {
                System.IO.File.AppendAllText(LogPath, "Services is null, creating manually\r\n");
                var sc = new ServiceCollection();
                sc.AddSingleton<ISettingsService, SettingsService>();
                sc.AddSingleton<IThemeService, ThemeService>();
                sc.AddSingleton<ISpotifyService, SpotifyService>();
                sc.AddSingleton<MainWindow>();
                sc.AddTransient<Controls.SettingsWindow>();
                sc.AddSingleton<SpotifyPlayerViewModel>();
                _serviceProvider = sc.BuildServiceProvider();
                Services = _serviceProvider;
            }
            System.IO.File.AppendAllText(LogPath, "Before GetRequiredService\r\n");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            _window = Services.GetRequiredService<MainWindow>();
            System.IO.File.AppendAllText(LogPath, $"Window created in {sw.ElapsedMilliseconds}ms\r\n");
            sw.Restart();
            _window.Activate();
            System.IO.File.AppendAllText(LogPath, $"After Activate in {sw.ElapsedMilliseconds}ms\r\n");
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText(LogPath, $"OnLaunched failed: {ex.GetType().Name}: {ex.Message}\r\n");
            throw;
        }
    }

    public void DisposeHost()
    {
        _serviceProvider?.Dispose();
    }
}