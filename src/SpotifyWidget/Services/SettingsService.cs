using System.Text.Json;
using SpotifyWidget.Models;

namespace SpotifyWidget.Services;

public interface ISettingsService
{
    WidgetSettings GetSettings();
    Task SaveSettingsAsync(WidgetSettings settings);
    Task LoadSettingsAsync();
}

public class SettingsService : ISettingsService
{
    private WidgetSettings _settings = new();
    private readonly string _settingsPath;

    public SettingsService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var settingsDir = Path.Combine(appData, "SpotifyWidget");
        Directory.CreateDirectory(settingsDir);
        _settingsPath = Path.Combine(settingsDir, "settings.json");
    }

    public WidgetSettings GetSettings()
    {
        return _settings;
    }

    public async Task SaveSettingsAsync(WidgetSettings settings)
    {
        _settings = settings;
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_settingsPath, json);
    }

    public async Task LoadSettingsAsync()
    {
        if (File.Exists(_settingsPath))
        {
            var json = await File.ReadAllTextAsync(_settingsPath);
            _settings = JsonSerializer.Deserialize<WidgetSettings>(json) ?? new WidgetSettings();
        }
    }
}
