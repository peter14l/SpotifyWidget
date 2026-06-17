using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SpotifyWidget.Models;
using SpotifyWidget.Services;

namespace SpotifyWidget.Controls;

public sealed partial class SettingsWindow : Window
{
    private readonly ISettingsService _settingsService;
    private readonly IThemeService _themeService;

    public SettingsWindow(ISettingsService settingsService, IThemeService themeService)
    {
        this.InitializeComponent();
        _settingsService = settingsService;
        _themeService = themeService;

        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = _settingsService.GetSettings();

        ClientIdTextBox.Text = settings.ClientId;
        ThemeCombo.SelectedIndex = settings.Theme switch
        {
            "Light" => 1,
            "Dark" => 2,
            _ => 0
        };
        RefreshIntervalTextBox.Text = settings.RefreshIntervalMs.ToString();
        OpacitySlider.Value = settings.Opacity;
        AlwaysOnTopCheckbox.IsChecked = settings.AlwaysOnTop;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var settings = _settingsService.GetSettings();

        settings.ClientId = ClientIdTextBox.Text;
        settings.Theme = (ThemeCombo.SelectedIndex) switch
        {
            1 => "Light",
            2 => "Dark",
            _ => "System"
        };
        settings.RefreshIntervalMs = int.TryParse(RefreshIntervalTextBox.Text, out var interval) ? interval : 1000;
        settings.Opacity = OpacitySlider.Value;
        settings.AlwaysOnTop = AlwaysOnTopCheckbox.IsChecked ?? true;

        await _settingsService.SaveSettingsAsync(settings);

        var theme = settings.Theme switch
        {
            "Dark" => ElementTheme.Dark,
            "Light" => ElementTheme.Light,
            _ => ElementTheme.Default
        };
        _themeService.SetTheme(theme);

        this.Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}