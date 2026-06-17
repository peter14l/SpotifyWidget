using Microsoft.UI.Xaml;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace SpotifyWidget.Services;

public interface IThemeService
{
    ElementTheme GetCurrentTheme();
    void SetTheme(ElementTheme theme);
    Color GetTintColor();
    void SaveWindowPosition(Point position);
    Point LoadWindowPosition();
}

public class ThemeService : IThemeService
{
    private readonly ISettingsService _settingsService;
    private ElementTheme _currentTheme = ElementTheme.Default;

    public ThemeService(ISettingsService settingsService)
    {
        System.IO.File.AppendAllText(@"C:\Users\LOQ\AppData\Local\Temp\app_trace.log", "ThemeService ctor\r\n");
        _settingsService = settingsService;
    }

    public ElementTheme GetCurrentTheme()
    {
        if (_currentTheme != ElementTheme.Default)
            return _currentTheme;

        var settings = new UISettings();
        var backgroundColor = settings.GetColorValue(UIColorType.Background);
        var isDark = backgroundColor.R < 128 && backgroundColor.G < 128 && backgroundColor.B < 128;
        return isDark ? ElementTheme.Dark : ElementTheme.Light;
    }

    public void SetTheme(ElementTheme theme)
    {
        _currentTheme = theme;
        var settings = _settingsService.GetSettings();
        settings.Theme = theme switch
        {
            ElementTheme.Dark => "Dark",
            ElementTheme.Light => "Light",
            _ => "System"
        };
        _ = _settingsService.SaveSettingsAsync(settings);
    }

    public Color GetTintColor()
    {
        var isDark = GetCurrentTheme() == ElementTheme.Dark;
        return isDark ? Color.FromArgb(30, 32, 32, 32) : Color.FromArgb(230, 249, 249, 249);
    }

    public void SaveWindowPosition(Point position)
    {
        var settings = _settingsService.GetSettings();
        settings.WindowX = position.X;
        settings.WindowY = position.Y;
        _ = _settingsService.SaveSettingsAsync(settings);
    }

    public Point LoadWindowPosition()
    {
        var settings = _settingsService.GetSettings();
        return new Point(settings.WindowX, settings.WindowY);
    }
}