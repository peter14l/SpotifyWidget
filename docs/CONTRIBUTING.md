# Contributing to SpotifyWidget

Thank you for your interest in contributing! This document provides guidelines and information for contributors.

## Getting Started

### Prerequisites

- Windows 10/11
- .NET 10.0 SDK
- Visual Studio 2022 (17.8+)
- Git
- Spotify Developer Account (for testing)

### Setting Up Development Environment

1. **Fork the repository**
   ```bash
   # Fork on GitHub, then clone
   git clone https://github.com/YOUR_USERNAME/SpotifyWidget.git
   cd SpotifyWidget
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure Spotify**
   - Create a Spotify Developer App at https://developer.spotify.com/dashboard
   - Add `http://localhost:52763/callback` as a redirect URI
   - Update `ClientId` in `src/SpotifyWidget/Services/SpotifyService.cs`

4. **Build and run**
   ```bash
   dotnet build
   dotnet run --project src/SpotifyWidget
   ```

## Development Workflow

### Branching Strategy

- `main` - Stable release branch
- `develop` - Integration branch
- `feature/*` - Feature branches
- `fix/*` - Bug fix branches
- `release/*` - Release preparation

### Creating a Branch

```bash
git checkout develop
git pull origin develop
git checkout -b feature/your-feature-name
```

### Making Changes

1. **Follow code style**
   - Use `.editorconfig` settings
   - Run `dotnet format` before committing

2. **Write meaningful commits**
   ```
   feat: add volume control slider
   fix: resolve album art loading issue
   docs: update architecture guide
   ```

3. **Keep changes focused**
   - One feature/fix per commit
   - Avoid unrelated changes

### Testing

```bash
# Run all tests
dotnet test

# Run specific tests
dotnet test --filter "FullyQualifiedName~ClassName"
```

### Submitting Changes

1. **Push your branch**
   ```bash
   git push origin feature/your-feature-name
   ```

2. **Create a Pull Request**
   - Target `develop` branch
   - Fill out PR template
   - Link related issues

3. **Code Review**
   - Address review comments
   - Make requested changes
   - Get approval from maintainers

## Code Guidelines

### C# Style

```csharp
// Use descriptive names
public async Task<SpotifyPlaybackState?> GetPlaybackStateAsync()
{
    // Use var when type is obvious
    var response = await _httpClient.GetAsync(url);
    
    // Pattern matching
    if (response is { IsSuccessStatusCode: true })
    {
        // Handle success
    }
    
    // Null-conditional operator
    var trackName = state?.Track?.Name ?? "Unknown";
}
```

### XAML Style

```xml
<!-- Use meaningful names -->
<StackPanel x:Name="PlayerControls"
            Orientation="Horizontal"
            Spacing="16">
    
    <!-- Use static resources -->
    <Button Style="{StaticResource WidgetButtonStyle}"
            Content="&#xE768;" />
</StackPanel>
```

### Naming Conventions

| Type | Convention | Example |
|------|-----------|---------|
| Classes | PascalCase | `SpotifyService` |
| Methods | PascalCase | `GetPlaybackStateAsync` |
| Properties | PascalCase | `IsPlaying` |
| Fields | _camelCase | `_httpClient` |
| Parameters | camelCase | `positionMs` |
| Local variables | camelCase | `trackName` |

## Reporting Issues

### Bug Reports

Use the bug report template and include:

1. **Environment**
   - Windows version
   - .NET version
   - App version

2. **Steps to reproduce**
   - Clear, numbered steps
   - Minimal reproduction case

3. **Expected behavior**
   - What should happen

4. **Actual behavior**
   - What actually happens
   - Error messages/screenshots

### Feature Requests

Use the feature request template and include:

1. **Problem statement**
   - What problem does this solve?

2. **Proposed solution**
   - How should it work?

3. **Alternatives considered**
   - Other approaches?

## Documentation

### Code Comments

```csharp
/// <summary>
/// Gets the current playback state from Spotify API.
/// </summary>
/// <returns>Current playback state or null if not authenticated.</returns>
/// <exception cref="HttpRequestException">Thrown when API request fails.</exception>
public async Task<SpotifyPlaybackState?> GetPlaybackStateAsync()
{
    // Implementation
}
```

### Documentation Updates

- Update `docs/ARCHITECTURE.md` for architectural changes
- Update `docs/PROGRESS.md` for feature completion
- Update `README.md` for user-facing changes

## Code of Conduct

### Our Standards

- Be respectful and inclusive
- Focus on constructive feedback
- Help newcomers learn
- Celebrate contributions

### Unacceptable Behavior

- Harassment or discrimination
- Spam or off-topic content
- Malicious contributions
- Private information sharing

## Questions?

- Open a discussion on GitHub
- Check existing documentation
- Review closed issues for similar questions

Thank you for contributing! 🎵
