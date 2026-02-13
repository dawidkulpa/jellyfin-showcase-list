using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.ShowcaseList.Configuration;

public sealed class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Where to write the list of item IDs (one per line).
    /// </summary>
    public string OutputPath { get; set; } = "/jellyfin/jellyfin-web/list.txt";

    /// <summary>
    /// The maximum number of IDs to write.
    /// Fixed at 10 per your request, but still kept in config for future flexibility.
    /// </summary>
    public int MaxItems { get; set; } = 10;

    /// <summary>
    /// Debounce window for library-change storms (e.g., initial scans).
    /// </summary>
    public int DebounceSeconds { get; set; } = 30;

    /// <summary>
    /// If true, keep items without rating (score=0) so the list always fills MaxItems.
    /// </summary>
    public bool IncludeUnratedAsZero { get; set; } = true;
}
