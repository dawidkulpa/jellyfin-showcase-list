using System;
using Jellyfin.Plugin.ShowcaseList.Services;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ShowcaseList;

/// <summary>
/// Subscribes to library events to keep the list updated.
/// </summary>
public sealed class ShowcaseEntryPoint : IServerEntryPoint
{
    private readonly ILogger<ShowcaseEntryPoint> _logger;
    private readonly ILibraryManager _libraryManager;
    private readonly ShowcaseListManager _manager;

    public ShowcaseEntryPoint(
        ILogger<ShowcaseEntryPoint> logger,
        ILibraryManager libraryManager,
        ShowcaseListManager manager)
    {
        _logger = logger;
        _libraryManager = libraryManager;
        _manager = manager;
    }

    public void Run()
    {
        _logger.LogInformation("ShowcaseList entry point starting");

        _libraryManager.ItemAdded += OnLibraryItemChanged;
        _libraryManager.ItemUpdated += OnLibraryItemChanged;
        _libraryManager.ItemRemoved += OnLibraryItemRemoved;

        // Initial build on server start.
        _manager.ScheduleRebuild("startup");
    }

    public void Dispose()
    {
        _logger.LogInformation("ShowcaseList entry point disposing");

        _libraryManager.ItemAdded -= OnLibraryItemChanged;
        _libraryManager.ItemUpdated -= OnLibraryItemChanged;
        _libraryManager.ItemRemoved -= OnLibraryItemRemoved;
    }

    private void OnLibraryItemChanged(object? sender, ItemChangeEventArgs e)
    {
        // Recompute the top list on any relevant item changes.
        _manager.ScheduleRebuild($"library:{e.ChangeType}");
    }

    private void OnLibraryItemRemoved(object? sender, ItemChangeEventArgs e)
    {
        _manager.ScheduleRebuild($"library:removed");
    }
}
