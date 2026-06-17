using System.Security.Cryptography;
using System.Text;
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
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("SpotifyWidget-Token-Protection");

    public SettingsService()
    {
        System.IO.File.AppendAllText(@"C:\Users\LOQ\AppData\Local\Temp\app_trace.log", "SettingsService ctor start\r\n");
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var settingsDir = Path.Combine(appData, "SpotifyWidget");
        Directory.CreateDirectory(settingsDir);
        _settingsPath = Path.Combine(settingsDir, "settings.json");
        System.IO.File.AppendAllText(@"C:\Users\LOQ\AppData\Local\Temp\app_trace.log", "SettingsService ctor done\r\n");
    }

    public WidgetSettings GetSettings()
    {
        return _settings;
    }

    public async Task SaveSettingsAsync(WidgetSettings settings)
    {
        _settings = settings;

        var serializable = new WidgetSettingsSerializable
        {
            WindowX = settings.WindowX,
            WindowY = settings.WindowY,
            WindowWidth = settings.WindowWidth,
            WindowHeight = settings.WindowHeight,
            Theme = settings.Theme,
            AlwaysOnTop = settings.AlwaysOnTop,
            ShowAlbumArt = settings.ShowAlbumArt,
            ShowProgressBar = settings.ShowProgressBar,
            ShowControls = settings.ShowControls,
            RefreshIntervalMs = settings.RefreshIntervalMs,
            Opacity = settings.Opacity,
            ClientId = settings.ClientId,
            EncryptedRefreshToken = !string.IsNullOrEmpty(settings.RefreshToken)
                ? EncryptToken(settings.RefreshToken)
                : string.Empty
        };

        var json = JsonSerializer.Serialize(serializable, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_settingsPath, json);
    }

    public async Task LoadSettingsAsync()
    {
        if (File.Exists(_settingsPath))
        {
            var json = await File.ReadAllTextAsync(_settingsPath);
            var serializable = JsonSerializer.Deserialize<WidgetSettingsSerializable>(json);
            if (serializable != null)
            {
                _settings.WindowX = serializable.WindowX;
                _settings.WindowY = serializable.WindowY;
                _settings.WindowWidth = serializable.WindowWidth;
                _settings.WindowHeight = serializable.WindowHeight;
                _settings.Theme = serializable.Theme;
                _settings.AlwaysOnTop = serializable.AlwaysOnTop;
                _settings.ShowAlbumArt = serializable.ShowAlbumArt;
                _settings.ShowProgressBar = serializable.ShowProgressBar;
                _settings.ShowControls = serializable.ShowControls;
                _settings.RefreshIntervalMs = serializable.RefreshIntervalMs;
                _settings.Opacity = serializable.Opacity;
                _settings.ClientId = serializable.ClientId;

                if (!string.IsNullOrEmpty(serializable.EncryptedRefreshToken))
                {
                    _settings.RefreshToken = DecryptToken(serializable.EncryptedRefreshToken);
                }
            }
        }
    }

    private static string EncryptToken(string token)
    {
        try
        {
            var plainBytes = Encoding.UTF8.GetBytes(token);
            var encryptedBytes = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedBytes);
        }
        catch
        {
            return token;
        }
    }

    private static string DecryptToken(string encryptedToken)
    {
        try
        {
            var encryptedBytes = Convert.FromBase64String(encryptedToken);
            var plainBytes = ProtectedData.Unprotect(encryptedBytes, Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    private class WidgetSettingsSerializable
    {
        public double WindowX { get; set; } = 100;
        public double WindowY { get; set; } = 100;
        public double WindowWidth { get; set; } = 360;
        public double WindowHeight { get; set; } = 280;
        public string Theme { get; set; } = "System";
        public bool AlwaysOnTop { get; set; } = true;
        public bool ShowAlbumArt { get; set; } = true;
        public bool ShowProgressBar { get; set; } = true;
        public bool ShowControls { get; set; } = true;
        public int RefreshIntervalMs { get; set; } = 1000;
        public double Opacity { get; set; } = 1.0;
        public string ClientId { get; set; } = string.Empty;
        public string EncryptedRefreshToken { get; set; } = string.Empty;
    }
}