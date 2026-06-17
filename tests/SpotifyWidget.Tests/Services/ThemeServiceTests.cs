using Moq;
using Xunit;
using SpotifyWidget.Models;
using SpotifyWidget.Services;

namespace SpotifyWidget.Tests.Services;

public class ThemeServiceTests
{
    [Fact]
    public void SetTheme_UpdatesSettings()
    {
        var mockSettings = new Mock<ISettingsService>();
        var savedSettings = new WidgetSettings();
        mockSettings.Setup(s => s.GetSettings()).Returns(savedSettings);
        mockSettings.Setup(s => s.SaveSettingsAsync(It.IsAny<WidgetSettings>()))
            .Callback<WidgetSettings>(s => savedSettings = s)
            .Returns(Task.CompletedTask);

        var service = new ThemeService(mockSettings.Object);

        service.SetTheme(Microsoft.UI.Xaml.ElementTheme.Dark);

        Assert.Equal("Dark", savedSettings.Theme);
        mockSettings.Verify(s => s.SaveSettingsAsync(It.IsAny<WidgetSettings>()), Times.Once);
    }

    [Fact]
    public void SetTheme_Light_SavesCorrectly()
    {
        var mockSettings = new Mock<ISettingsService>();
        var savedSettings = new WidgetSettings();
        mockSettings.Setup(s => s.GetSettings()).Returns(savedSettings);
        mockSettings.Setup(s => s.SaveSettingsAsync(It.IsAny<WidgetSettings>()))
            .Callback<WidgetSettings>(s => savedSettings = s)
            .Returns(Task.CompletedTask);

        var service = new ThemeService(mockSettings.Object);

        service.SetTheme(Microsoft.UI.Xaml.ElementTheme.Light);

        Assert.Equal("Light", savedSettings.Theme);
    }

    [Fact]
    public void SetTheme_Default_SavesAsSystem()
    {
        var mockSettings = new Mock<ISettingsService>();
        var savedSettings = new WidgetSettings();
        mockSettings.Setup(s => s.GetSettings()).Returns(savedSettings);
        mockSettings.Setup(s => s.SaveSettingsAsync(It.IsAny<WidgetSettings>()))
            .Callback<WidgetSettings>(s => savedSettings = s)
            .Returns(Task.CompletedTask);

        var service = new ThemeService(mockSettings.Object);

        service.SetTheme(Microsoft.UI.Xaml.ElementTheme.Default);

        Assert.Equal("System", savedSettings.Theme);
    }

    [Fact]
    public void SaveWindowPosition_PersistsCoordinates()
    {
        var mockSettings = new Mock<ISettingsService>();
        var savedSettings = new WidgetSettings();
        mockSettings.Setup(s => s.GetSettings()).Returns(savedSettings);
        mockSettings.Setup(s => s.SaveSettingsAsync(It.IsAny<WidgetSettings>()))
            .Callback<WidgetSettings>(s => savedSettings = s)
            .Returns(Task.CompletedTask);

        var service = new ThemeService(mockSettings.Object);

        service.SaveWindowPosition(new Windows.Foundation.Point(500, 400));

        Assert.Equal(500, savedSettings.WindowX);
        Assert.Equal(400, savedSettings.WindowY);
    }

    [Fact]
    public void LoadWindowPosition_ReturnsSavedPosition()
    {
        var mockSettings = new Mock<ISettingsService>();
        var savedSettings = new WidgetSettings { WindowX = 300, WindowY = 200 };
        mockSettings.Setup(s => s.GetSettings()).Returns(savedSettings);

        var service = new ThemeService(mockSettings.Object);

        var position = service.LoadWindowPosition();

        Assert.Equal(300, position.X);
        Assert.Equal(200, position.Y);
    }

    [Fact]
    public void LoadWindowPosition_ReturnsDefaultWhenNotSet()
    {
        var mockSettings = new Mock<ISettingsService>();
        var savedSettings = new WidgetSettings();
        mockSettings.Setup(s => s.GetSettings()).Returns(savedSettings);

        var service = new ThemeService(mockSettings.Object);

        var position = service.LoadWindowPosition();

        Assert.Equal(100, position.X);
        Assert.Equal(100, position.Y);
    }
}