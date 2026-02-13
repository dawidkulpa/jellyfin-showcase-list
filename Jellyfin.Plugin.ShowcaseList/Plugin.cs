using System;
using System.Collections.Generic;
using Jellyfin.Plugin.ShowcaseList.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.ShowcaseList;

public sealed class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public static Plugin? Instance { get; private set; }

    public override string Name => "Showcase List";

    public override Guid Id => Guid.Parse("2d3f6a3e-8b5d-4f43-8d61-7df8c8d3a1b0");

    public IEnumerable<PluginPageInfo> GetPages()
    {
        yield return new PluginPageInfo
        {
            Name = "configPage",
            EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html",
        };
    }
}
