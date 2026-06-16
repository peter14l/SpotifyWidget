# SpotifyWidget

A modern, lightweight Spotify widget for Windows desktop built with WinUI 3 and Mica material design.

![CI](https://github.com/peter14l/SpotifyWidget/actions/workflows/ci.yml/badge.svg)
![License](https://img.shields.io/github/license/peter14l/SpotifyWidget)
![Release](https://img.shields.io/github/v/release/peter14l/SpotifyWidget)

## Features

- 🎵 **Real-time Spotify Integration** - Control playback, view track info, album art
- 🎨 **Mica Material Design** - Native Windows 11 aesthetic with translucent backdrop
- 📌 **Desktop Widget** - Always-on-top, hidden from taskbar, floats on desktop
- 🎮 **Full Playback Controls** - Play/pause, next/previous, seek, volume
- 🎨 **Dynamic Theming** - Automatic dark/light theme based on system settings
- 💾 **Persistent Settings** - Remembers window position and preferences

## Requirements

- Windows 10 (version 1903+) or Windows 11
- .NET 10.0 SDK
- Spotify Premium account
- Spotify Developer App (for API access)

## Getting Started

### Prerequisites

1. Install [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
2. Create a [Spotify Developer App](https://developer.spotify.com/dashboard)
3. Add `http://localhost:52763/callback` as a redirect URI in your Spotify app settings

### Build & Run

```bash
# Clone the repository
git clone https://github.com/peter14l/SpotifyWidget.git
cd SpotifyWidget

# Restore dependencies
dotnet restore

# Build
dotnet build --configuration Release

# Run
dotnet run --project src/SpotifyWidget
```

### Configuration

1. Open `src/SpotifyWidget/Services/SpotifyService.cs`
2. Replace `YOUR_SPOTIFY_CLIENT_ID` with your actual Spotify Client ID
3. Build and run the application

## Architecture

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for detailed architecture documentation.

```
SpotifyWidget/
├── src/SpotifyWidget/           # Main application
│   ├── Controls/                # Custom UI controls
│   ├── Models/                  # Data models
│   ├── Services/                # Business logic & APIs
│   ├── ViewModels/              # MVVM view models
│   ├── Views/                   # XAML pages
│   ├── Styles/                  # Resource dictionaries
│   └── Assets/                  # Images & icons
├── docs/                        # Documentation
│   ├── ARCHITECTURE.md          # Architecture guide
│   ├── PROGRESS.md              # Development progress
│   └── CONTRIBUTING.md          # Contribution guidelines
└── .github/workflows/           # CI/CD pipelines
```

## Development

See [docs/PROGRESS.md](docs/PROGRESS.md) for current development status and roadmap.

### Tech Stack

- **Framework**: .NET 10.0
- **UI**: WinUI 3 (Windows App SDK 1.7)
- **Design**: Mica Material, Fluent Design
- **MVVM**: CommunityToolkit.Mvvm
- **HTTP**: System.Net.Http
- **Serialization**: System.Text.Json

## Contributing

See [docs/CONTRIBUTING.md](docs/CONTRIBUTING.md) for guidelines.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [WinUI 3](https://github.com/microsoft/microsoft-ui-xaml) - Modern Windows UI framework
- [Windows App SDK](https://github.com/microsoft/WindowsAppSDK) - Windows application platform
- [Spotify Web API](https://developer.spotify.com/documentation/web-api) - Music streaming API
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/MVVM-Samples) - MVVM framework
