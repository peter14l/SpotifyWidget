using Microsoft.UI.Xaml;
using SpotifyWidget.Models;

namespace SpotifyWidget.Services;

public interface IThemeService
{
    ElementTheme GetCurrentTheme();
    void SetTheme(ElementTheme theme);
    Windows.Foundation.Point GetTintColor();
    void SaveWindowPosition(Windows.Foundation.Point position);
    Windows.Foundation.Point LoadWindowPosition();
}

public class ThemeService : IThemeService
{
    private ElementTheme _currentTheme = ElementTheme.Dark;

    public ElementTheme GetCurrentTheme()
    {
        return _currentTheme;
    }

    public void SetTheme(ElementTheme theme)
    {
        _currentTheme = theme;
    }

    public Windows.Foundation.Point GetTintColor()
    {
        return new Windows.Foundation.Point(30, 30);
    }

    public void SaveWindowPosition(Windows.Foundation.Point position)
    {
        var settings = new WidgetSettings
        {
            WindowX = position.X,
            WindowY = position.Y
        };
    }

    public Windows.Foundation.Point LoadWindowPosition()
    {
        return new Windows.Foundation.Point(100, 100);
    }
}
