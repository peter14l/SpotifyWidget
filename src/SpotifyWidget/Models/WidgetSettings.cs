namespace SpotifyWidget.Models;

public class WidgetSettings
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
}
