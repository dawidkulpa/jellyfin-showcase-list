using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Jellyfin.Plugin.ShowcaseList.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ShowcaseList.Services;

/// <summary>
/// Computes and writes the Top-N rated items list for your home page showcase.
/// </summary>
public sealed class ShowcaseListManager : IDisposable
{
    private readonly ILogger<ShowcaseListManager> _logger;
    private readonly ILibraryManager _libraryManager;

    private readonly object _gate = new();
    private Timer? _debounceTimer;

    public ShowcaseListManager(
        ILogger<ShowcaseListManager> logger,
        ILibraryManager libraryManager)
    {
        _logger = logger;
        _libraryManager = libraryManager;
    }

    private PluginConfiguration Config => Plugin.Instance?.Configuration ?? new PluginConfiguration();

    public void Dispose()
    {
        lock (_gate)
        {
            _debounceTimer?.Dispose();
            _debounceTimer = null;
        }
    }

    public void ScheduleRebuild(string reason)
    {
        var debounceSeconds = Math.Max(0, Config.DebounceSeconds);
        _logger.LogInformation("Scheduling showcase list rebuild in {DebounceSeconds}s (reason: {Reason})", debounceSeconds, reason);

        lock (_gate)
        {
            _debounceTimer?.Dispose();
            _debounceTimer = new Timer(
                _ =>
                {
                    try
                    {
                        RebuildNow("debounced:" + reason);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to rebuild showcase list");
                    }
                },
                null,
                TimeSpan.FromSeconds(debounceSeconds),
                Timeout.InfiniteTimeSpan);
        }
    }

    public void RebuildNow(string reason)
    {
        _logger.LogInformation("Rebuilding showcase list now (reason: {Reason})", reason);

        var maxItems = Math.Clamp(Config.MaxItems, 1, 100);
        var includeUnrated = Config.IncludeUnratedAsZero;

        // Query movies + series.
        // NOTE: This uses internal query APIs that are stable across Jellyfin versions, but may require tweaks
        // depending on 10.11.x package surface.
        var query = new InternalItemsQuery
        {
            Recursive = true,
            IncludeItemTypes = new[]
            {
                BaseItemKind.Movie,
                BaseItemKind.Series
            }
        };

        var items = _libraryManager.GetItemList(query);

        var ranked = items
            .Select(i => new RankedItem(i, GetScore(i)))
            .Where(x => includeUnrated || x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Item.DateCreated ?? DateTime.MinValue)
            .Take(maxItems)
            .ToList();

        WriteIdsFile(ranked.Select(r => r.Item.Id.ToString("N", CultureInfo.InvariantCulture)).ToList());

        _logger.LogInformation(
            "Showcase list updated: wrote {Count} ids to {Path}. Top score: {TopScore}",
            ranked.Count,
            Config.OutputPath,
            ranked.Count > 0 ? ranked[0].Score : 0);
    }

    public IReadOnlyList<(string Name, string Id, double Score)> PreviewTop(string? reason = null)
    {
        var maxItems = Math.Clamp(Config.MaxItems, 1, 100);

        var query = new InternalItemsQuery
        {
            Recursive = true,
            IncludeItemTypes = new[] { BaseItemKind.Movie, BaseItemKind.Series }
        };

        var items = _libraryManager.GetItemList(query);

        return items
            .Select(i => new RankedItem(i, GetScore(i)))
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Item.DateCreated ?? DateTime.MinValue)
            .Take(maxItems)
            .Select(x => (x.Item.Name ?? "(no title)", x.Item.Id.ToString("N", CultureInfo.InvariantCulture), x.Score))
            .ToList();
    }

    private double GetScore(BaseItem item)
    {
        // CommunityRating is typically 0-10 (often sourced from IMDb/TMDb/etc depending on your providers).
        var community = item.CommunityRating;
        if (community.HasValue)
        {
            return ClampScore(community.Value);
        }

        // CriticRating is typically 0-100.
        var critic = item.CriticRating;
        if (critic.HasValue)
        {
            return ClampScore(critic.Value / 10.0);
        }

        return 0;
    }

    private static double ClampScore(double score)
        => Math.Clamp(score, 0, 10);

    private void WriteIdsFile(IReadOnlyList<string> ids)
    {
        var path = Config.OutputPath;
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tmp = path + ".tmp";
        var content = string.Join("\n", ids) + "\n";

        // Atomic-ish write: write temp, then replace.
        File.WriteAllText(tmp, content, new UTF8Encoding(false));

        // Replace if exists, else move.
        if (File.Exists(path))
        {
            File.Replace(tmp, path, destinationBackupFileName: null);
        }
        else
        {
            File.Move(tmp, path);
        }
    }

    private sealed record RankedItem(BaseItem Item, double Score);
}
