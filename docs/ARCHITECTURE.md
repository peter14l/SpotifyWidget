# Architecture

## Overview

SpotifyWidget is a modern Windows desktop widget built with WinUI 3 and Windows App SDK. It provides a lightweight, always-on-top interface for controlling Spotify playback directly from the desktop.

## Design Principles

1. **Native Windows Experience** - Uses WinUI 3 and Mica material for authentic Windows 11 feel
2. **Widget Behavior** - Floats on desktop, hidden from taskbar, always-on-top
3. **MVVM Pattern** - Clean separation of concerns with CommunityToolkit.Mvvm
4. **Performance** - Minimal resource usage, efficient polling

## Project Structure

```
SpotifyWidget/
├── src/SpotifyWidget/
│   ├── App.xaml(.cs)              # Application entry point
│   ├── MainWindow.xaml(.cs)       # Main window with Mica backdrop
│   ├── Controls/                  # Reusable UI components
│   │   └── SpotifyPlayerControl  # Main player UI
│   ├── Models/                    # Data structures
│   │   ├── SpotifyModels.cs      # Spotify API models
│   │   └── WidgetSettings.cs     # User preferences
│   ├── Services/                  # Business logic
│   │   ├── ISpotifyService.cs    # Spotify interface
│   │   ├── SpotifyService.cs     # API integration
│   │   ├── IThemeService.cs      # Theme interface
│   │   ├── ThemeService.cs       # Theme management
│   │   ├── ISettingsService.cs   # Settings interface
│   │   ├── SettingsService.cs    # Persistent storage
│   │   └── ServiceCollection.cs  # DI container
│   ├── Styles/                    # XAML resources
│   │   └── WidgetStyles.xaml     # Widget-specific styles
│   └── Assets/                    # Images and icons
├── docs/                          # Documentation
└── .github/workflows/            # CI/CD
```

## Core Components

### 1. Widget Window Management

The widget behaves as a floating window with special properties:

- **Always-on-top**: Uses `OverlappedPresenter.IsAlwaysOnTop`
- **Hidden from taskbar**: `AppWindow.IsShownInSwitchers = false`
- **Mica backdrop**: `MicaController` with translucent material
- **Position persistence**: Saves/loads window position

```csharp
// Window configuration
_appWindow.IsShownInSwitchers = false;
presenter.IsAlwaysOnTop = true;
presenter.IsResizable = true;
```

### 2. Spotify Integration

Uses Spotify Web API for playback control:

- **Authentication**: OAuth 2.0 with PKCE flow
- **Playback State**: Real-time polling via `/v1/me/player`
- **Controls**: Play, pause, next, previous, seek, volume

```csharp
// API endpoints
GET  /v1/me/player          // Current state
PUT  /v1/me/player/play     // Play
PUT  /v1/me/player/pause    // Pause
POST /v1/me/player/next     // Next track
POST /v1/me/player/previous // Previous track
```

### 3. Service Architecture

Dependency injection with `Microsoft.Extensions.DependencyInjection`:

```csharp
services.AddSingleton<ISettingsService, SettingsService>();
services.AddSingleton<IThemeService, ThemeService>();
services.AddSingleton<ISpotifyService, SpotifyService>();
services.AddSingleton<MainWindow>();
services.AddSingleton<SpotifyPlayerViewModel>();
```

### 4. Theme System

Automatic dark/light theme support:

- System theme detection
- Mica material tinting
- Accent color integration

## Data Flow

```
┌─────────────────────────────────────────────────────┐
│                    MainWindow                        │
│  ┌───────────────────────────────────────────────┐  │
│  │              Mica Backdrop                     │  │
│  │  ┌─────────────────────────────────────────┐  │  │
│  │  │         SpotifyPlayerControl             │  │  │
│  │  │  ┌─────────┐  ┌─────────────────────┐  │  │  │
│  │  │  │ Album   │  │ Track Info          │  │  │  │
│  │  │  │ Art     │  │ - Name              │  │  │  │
│  │  │  │         │  │ - Artist            │  │  │  │
│  │  │  └─────────┘  └─────────────────────┘  │  │  │
│  │  │  ┌─────────────────────────────────┐   │  │  │
│  │  │  │ Progress Bar                    │   │  │  │
│  │  │  └─────────────────────────────────┘   │  │  │
│  │  │  ┌─────┐ ┌─────┐ ┌─────┐              │  │  │
│  │  │  │ ◄◄  │ │ ▶︎   │ │ ►►  │              │  │  │
│  │  │  └─────┘ └─────┘ └─────┘              │  │  │
│  │  └─────────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────┐
│              SpotifyService                          │
│  - OAuth 2.0 Authentication                         │
│  - Playback State Polling                           │
│  - API Command Execution                            │
└─────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────┐
│              Spotify Web API                         │
│  https://api.spotify.com/v1/                        │
└─────────────────────────────────────────────────────┘
```

## Window Behavior

### Widget Mode

The window operates in a special "widget mode":

1. **Z-Order Management**: Stays behind other windows but above desktop
2. **Focus Handling**: Doesn't steal focus from active applications
3. **Position Persistence**: Saves position on move, restores on launch
4. **Size Constraints**: Minimum/maximum size limits

### Mica Backdrop

```csharp
private bool TrySetMicaBackdrop()
{
    if (MicaController.IsSupported())
    {
        var micaController = new MicaController();
        micaController.Kind = MicaKind.Base;
        
        var backdropConfiguration = new SystemBackdropConfiguration
        {
            IsInputActive = true,
            TintColor = themeService.GetTintColor(),
            TintOpacity = 0.8,
            LuminosityOpacity = 0.4
        };
        
        micaController.AddSystemBackdropTarget(
            this.As<ICompositionSupportsSystemBackdrop>());
        micaController.SetSystemBackdropConfiguration(backdropConfiguration);
        
        return true;
    }
    return false;
}
```

## Security Considerations

1. **Token Storage**: OAuth tokens stored in app-local storage
2. **No Secrets in Code**: Client ID is configurable, not hardcoded
3. **HTTPS Only**: All API calls use TLS
4. **Minimal Permissions**: Requests only necessary Spotify scopes

## Performance

- **Polling Interval**: 1 second (configurable)
- **Memory Usage**: < 50MB typical
- **CPU Usage**: < 1% when idle
- **Startup Time**: < 500ms

## Future Enhancements

- [ ] Spotify Connect device switching
- [ ] Queue management
- [ ] Lyrics display
- [ ] Audio visualization
- [ ] Multiple widget layouts
- [ ] System tray integration
