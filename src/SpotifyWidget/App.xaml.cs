using Microsoft.UI.Xaml;
using SpotifyWidget.Services;

namespace SpotifyWidget;

public partial class App : Application
{
    private Window? _window;
    private static IServiceProvider? _serviceProvider;

    public static T? GetService<T>() where T : class
    {
        return _serviceProvider?.GetService(typeof(T)) as T;
    }

    public App()
    {
        this.InitializeComponent();
        ConfigureServices();
    }

    private static void ConfigureServices()
    {
        var services = new ServiceCollection();
        var settingsService = new SettingsService();
        var themeService = new ThemeService();
        var widgetManager = new WidgetWindowManager();
        var spotifyService = new SpotifyService(settingsService);

        services.AddSingleton(settingsService);
        services.AddSingleton(themeService);
        services.AddSingleton(widgetManager);
        services.AddSingleton<ISpotifyService>(spotifyService);

        _serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }

    public static Window? MainWindow => GetService<WidgetWindowManager>()?.MainWindow;
}
