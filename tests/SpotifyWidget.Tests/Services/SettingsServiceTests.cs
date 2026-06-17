using Xunit;
using SpotifyWidget.Models;
using SpotifyWidget.Services;

namespace SpotifyWidget.Tests.Services;

public class SettingsServiceTests : IDisposable
{
    private readonly string _testDir;
    private readonly SettingsService _service;

    public SettingsServiceTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "SpotifyWidgetTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDir);
        Environment.SetEnvironmentVariable("LocalAppData", _testDir);
        _service = new SettingsService();
    }

    [Fact]
    public void GetSettings_ReturnsDefaultSettings()
    {
        var settings = _service.GetSettings();

        Assert.Equal(100, settings.WindowX);
        Assert.Equal(100, settings.WindowY);
        Assert.Equal(360, settings.WindowWidth);
        Assert.Equal(280, settings.WindowHeight);
        Assert.Equal("System", settings.Theme);
        Assert.Equal(1000, settings.RefreshIntervalMs);
        Assert.Equal(1.0, settings.Opacity);
        Assert.True(string.IsNullOrEmpty(settings.RefreshToken));
    }

    [Fact]
    public async Task SaveAndLoadSettings_PreservesValues()
    {
        var originalSettings = _service.GetSettings();
        originalSettings.WindowX = 200;
        originalSettings.WindowY = 300;
        originalSettings.WindowWidth = 400;
        originalSettings.WindowHeight = 500;
        originalSettings.Theme = "Dark";
        originalSettings.RefreshIntervalMs = 2000;
        originalSettings.Opacity = 0.8;
        originalSettings.AlwaysOnTop = false;
        originalSettings.RefreshToken = "test-refresh-token";
        originalSettings.ClientId = "test-client-id";

        await _service.SaveSettingsAsync(originalSettings);

        var loadedService = new SettingsService();
        await loadedService.LoadSettingsAsync();
        var loadedSettings = loadedService.GetSettings();

        Assert.Equal(200, loadedSettings.WindowX);
        Assert.Equal(300, loadedSettings.WindowY);
        Assert.Equal(400, loadedSettings.WindowWidth);
        Assert.Equal(500, loadedSettings.WindowHeight);
        Assert.Equal("Dark", loadedSettings.Theme);
        Assert.Equal(2000, loadedSettings.RefreshIntervalMs);
        Assert.Equal(0.8, loadedSettings.Opacity);
        Assert.False(loadedSettings.AlwaysOnTop);
        Assert.Equal("test-refresh-token", loadedSettings.RefreshToken);
        Assert.Equal("test-client-id", loadedSettings.ClientId);
    }

    [Fact]
    public async Task SaveAndLoadSettings_WithEmptyDefaults()
    {
        var settings = _service.GetSettings();

        await _service.SaveSettingsAsync(settings);

        var loadedService = new SettingsService();
        await loadedService.LoadSettingsAsync();
        var loaded = loadedService.GetSettings();

        Assert.Equal(100, loaded.WindowX);
        Assert.Equal(360, loaded.WindowWidth);
        Assert.Equal("System", loaded.Theme);
    }

    public void Dispose()
    {
        try { Directory.Delete(_testDir, recursive: true); } catch { }
    }
}