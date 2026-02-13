using Jellyfin.Plugin.ShowcaseList.Services;
using Microsoft.Extensions.DependencyInjection;
using MediaBrowser.Common.Plugins;

namespace Jellyfin.Plugin.ShowcaseList.Services;

/// <summary>
/// Registers plugin services into Jellyfin's DI container.
/// </summary>
public sealed class ServiceRegistrator : IPluginServiceRegistrator
{
    public void RegisterServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ShowcaseListManager>();
    }
}
