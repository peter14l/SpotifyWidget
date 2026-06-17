using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using SpotifyWidget.Services;
using SpotifyWidget.ViewModels;

namespace SpotifyWidget;

public partial class App : Application
{
    private Window? _window;
    private IHost? _host;

    public static IServiceProvider Services { get; private set; } = null!;

    public App()
    {
        this.InitializeComponent();
        ConfigureServices();
    }

    private void ConfigureServices()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<ISettingsService, SettingsService>();
                services.AddSingleton<IThemeService, ThemeService>();
                services.AddSingleton<ISpotifyService, SpotifyService>();
                services.AddSingleton<MainWindow>();
                services.AddTransient<Controls.SettingsWindow>();
                services.AddSingleton<SpotifyPlayerViewModel>();
            })
            .Build();

        Services = _host.Services;

        InitializeServices();
    }

    private async void InitializeServices()
    {
        var settingsService = Services.GetRequiredService<ISettingsService>();
        await settingsService.LoadSettingsAsync();

        var spotifyService = Services.GetRequiredService<ISpotifyService>();
        await spotifyService.IsAuthenticatedAsync();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = Services.GetRequiredService<MainWindow>();
        _window.Activate();
    }

    protected override void OnExit(object sender, object args)
    {
        _host?.Dispose();
        base.OnExit(sender, args);
    }
}