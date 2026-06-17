using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using Xunit;
using SpotifyWidget.Models;
using SpotifyWidget.Services;

namespace SpotifyWidget.Tests.Services;

public class SpotifyServiceTests
{
    private readonly Mock<ISettingsService> _mockSettings;
    private readonly Mock<HttpMessageHandler> _mockHandler;
    private readonly SpotifyService _service;

    public SpotifyServiceTests()
    {
        _mockSettings = new Mock<ISettingsService>();
        _mockSettings.Setup(s => s.GetSettings()).Returns(new WidgetSettings());
        _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(_mockHandler.Object);
        _service = new SpotifyService(_mockSettings.Object, httpClient);
    }

    [Fact]
    public async Task IsAuthenticatedAsync_NoTokens_ReturnsFalse()
    {
        var result = await _service.IsAuthenticatedAsync();
        Assert.False(result);
    }

    [Fact]
    public async Task GetPlaybackStateAsync_NotAuthenticated_ReturnsNull()
    {
        var result = await _service.GetPlaybackStateAsync();
        Assert.Null(result);
    }

    [Fact]
    public async Task PlayAsync_NotAuthenticated_ReturnsFalse()
    {
        var result = await _service.PlayAsync();
        Assert.False(result);
    }

    [Fact]
    public async Task PauseAsync_NotAuthenticated_ReturnsFalse()
    {
        var result = await _service.PauseAsync();
        Assert.False(result);
    }

    [Fact]
    public async Task NextTrackAsync_NotAuthenticated_ReturnsFalse()
    {
        var result = await _service.NextTrackAsync();
        Assert.False(result);
    }

    [Fact]
    public async Task PreviousTrackAsync_NotAuthenticated_ReturnsFalse()
    {
        var result = await _service.PreviousTrackAsync();
        Assert.False(result);
    }

    [Fact]
    public async Task SeekAsync_NotAuthenticated_ReturnsFalse()
    {
        var result = await _service.SeekAsync(30000);
        Assert.False(result);
    }

    [Fact]
    public async Task SetVolumeAsync_ClampsVolume()
    {
        var settings = _mockSettings.Object.GetSettings();
        settings.RefreshToken = "test";
        _mockSettings.Setup(s => s.GetSettings()).Returns(settings);

        var result = await _service.SetVolumeAsync(150);
        Assert.False(result);

        result = await _service.SetVolumeAsync(-10);
        Assert.False(result);

        result = await _service.SetVolumeAsync(50);
        Assert.False(result);
    }

    [Fact]
    public async Task GetPlaybackStateAsync_Authenticated_ReturnsState()
    {
        var state = new SpotifyPlaybackState
        {
            IsPlaying = true,
            ProgressMs = 50000,
            Track = new SpotifyTrack
            {
                Id = "track1",
                Name = "Test Track",
                Artists = new List<SpotifyArtist>
                {
                    new() { Id = "artist1", Name = "Test Artist" }
                },
                Album = new SpotifyAlbum
                {
                    Name = "Test Album",
                    Images = new List<SpotifyImage>
                    {
                        new() { Url = new Uri("https://example.com/art.jpg"), Height = 300, Width = 300 }
                    }
                },
                DurationMs = 200000
            }
        };

        var json = JsonSerializer.Serialize(state);
        SetupAuthFlow(json);

        var settings = _mockSettings.Object.GetSettings();
        settings.RefreshToken = "test-refresh-token";
        _mockSettings.Setup(s => s.GetSettings()).Returns(settings);
        _mockSettings.Setup(s => s.SaveSettingsAsync(It.IsAny<WidgetSettings>())).Returns(Task.CompletedTask);

        var result = await _service.GetPlaybackStateAsync();

        Assert.NotNull(result);
        Assert.True(result.IsPlaying);
        Assert.Equal("Test Track", result.Track?.Name);
        Assert.Equal("Test Artist", result.Track?.ArtistNames);
    }

    [Fact]
    public async Task GetPlaybackStateAsync_NoContent_ReturnsNull()
    {
        SetupAuthFlow(null, HttpStatusCode.NoContent);

        var settings = _mockSettings.Object.GetSettings();
        settings.RefreshToken = "test-refresh-token";
        _mockSettings.Setup(s => s.GetSettings()).Returns(settings);
        _mockSettings.Setup(s => s.SaveSettingsAsync(It.IsAny<WidgetSettings>())).Returns(Task.CompletedTask);

        var result = await _service.GetPlaybackStateAsync();
        Assert.Null(result);
    }

    [Fact]
    public async Task Logout_ClearsTokens()
    {
        var savedSettings = new WidgetSettings();
        _mockSettings.Setup(s => s.SaveSettingsAsync(It.IsAny<WidgetSettings>()))
            .Callback<WidgetSettings>(s => savedSettings = s)
            .Returns(Task.CompletedTask);

        await _service.LogoutAsync();

        Assert.True(string.IsNullOrEmpty(savedSettings.RefreshToken));
    }

    [Fact]
    public async Task GetDevicesAsync_NotAuthenticated_ReturnsEmpty()
    {
        var result = await _service.GetDevicesAsync();
        Assert.Empty(result);
    }

    [Fact]
    public async Task TransferPlaybackAsync_NotAuthenticated_ReturnsFalse()
    {
        var result = await _service.TransferPlaybackAsync("device1");
        Assert.False(result);
    }

    private void SetupMockTokenRefresh()
    {
        var tokenResponse = new SpotifyTokenResponse
        {
            AccessToken = "new-access-token",
            TokenType = "Bearer",
            ExpiresIn = 3600,
            Scope = "user-read-playback-state"
        };
        var tokenJson = JsonSerializer.Serialize(tokenResponse);

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri?.AbsoluteUri == "https://accounts.spotify.com/api/token"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(tokenJson)
            });
    }

    private void SetupAuthFlow(string? responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        SetupMockTokenRefresh();

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri?.AbsoluteUri == "https://api.spotify.com/v1/me/player"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = responseContent != null ? new StringContent(responseContent) : null
            });
    }
}