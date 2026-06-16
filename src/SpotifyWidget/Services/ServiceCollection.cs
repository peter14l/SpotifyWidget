using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using System.Collections.Generic;
using WinRT.Interop;

namespace SpotifyWidget.Services;

public class WidgetWindowManager
{
    private Window? _mainWindow;
    public Window? MainWindow => _mainWindow;

    public void Initialize(Window mainWindow)
    {
        _mainWindow = mainWindow;
    }
}

public class ServiceCollection
{
    private readonly Dictionary<Type, object> _services = new();

    public void AddSingleton<TInterface, TImplementation>()
        where TInterface : class
        where TImplementation : class, TInterface, new()
    {
        _services[typeof(TInterface)] = new TImplementation();
    }

    public void AddSingleton<TInterface>(TInterface instance)
        where TInterface : class
    {
        _services[typeof(TInterface)] = instance;
    }

    public T? GetService<T>() where T : class
    {
        _services.TryGetValue(typeof(T), out var service);
        return service as T;
    }

    public IServiceProvider BuildServiceProvider()
    {
        return new ServiceProvider(_services);
    }
}

internal class ServiceProvider : IServiceProvider
{
    private readonly Dictionary<Type, object> _services;

    public ServiceProvider(Dictionary<Type, object> services)
    {
        _services = services;
    }

    public object? GetService(Type serviceType)
    {
        _services.TryGetValue(serviceType, out var service);
        return service;
    }
}
