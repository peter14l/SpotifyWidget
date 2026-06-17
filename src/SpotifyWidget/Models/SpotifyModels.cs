using System.Text.Json.Serialization;

namespace SpotifyWidget.Models;

public class SpotifyTrack
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("artists")]
    public List<SpotifyArtist> Artists { get; set; } = new();

    [JsonPropertyName("album")]
    public SpotifyAlbum Album { get; set; } = new();

    [JsonPropertyName("duration_ms")]
    public int DurationMs { get; set; }

    public string ArtistNames => string.Join(", ", Artists.Select(a => a.Name));
    public string AlbumName => Album.Name;
    public Uri? AlbumArtUri => Album.Images.FirstOrDefault()?.Url;
}

public class SpotifyArtist
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class SpotifyAlbum
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("images")]
    public List<SpotifyImage> Images { get; set; } = new();
}

public class SpotifyImage
{
    [JsonPropertyName("url")]
    public Uri Url { get; set; } = null!;

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }
}

public class SpotifyPlaybackState
{
    [JsonPropertyName("is_playing")]
    public bool IsPlaying { get; set; }

    [JsonPropertyName("progress_ms")]
    public int ProgressMs { get; set; }

    [JsonPropertyName("item")]
    public SpotifyTrack? Track { get; set; }

    [JsonPropertyName("device")]
    public SpotifyDevice? Device { get; set; }
}

public class SpotifyDevice
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class SpotifyDevicesResponse
{
    [JsonPropertyName("devices")]
    public List<SpotifyDevice> Devices { get; set; } = new();
}

public class SpotifyTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
}
