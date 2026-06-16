using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SpotifyWidget.Services;
using SpotifyWidget.Views;

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
        services.AddSingleton<WidgetWindowManager>();
        services.AddSingleton<ISpotifyService, SpotifyService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        _serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }

    public static Window? MainWindow => GetService<WidgetWindowManager>()?.MainWindow;
}
