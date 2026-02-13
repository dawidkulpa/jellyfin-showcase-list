using System;
using System.Collections.Generic;
using System.Net.Mime;
using Jellyfin.Api.Constants;
using Jellyfin.Plugin.ShowcaseList.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ShowcaseList.Api;

[ApiController]
[Authorize(Policy = Policies.RequiresElevation)]
[Route("ShowcaseList")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class ShowcaseController : ControllerBase
{
    private readonly ILogger<ShowcaseController> _logger;
    private readonly ShowcaseListManager _manager;

    public ShowcaseController(ILogger<ShowcaseController> logger, ShowcaseListManager manager)
    {
        _logger = logger;
        _manager = manager;
    }

    [HttpPost("refresh")]
    public IActionResult RefreshNow()
    {
        _logger.LogInformation("Manual refresh requested");
        _manager.RebuildNow("manual");
        return Ok(new { ok = true });
    }

    [HttpGet("preview")]
    public ActionResult<IReadOnlyList<object>> Preview()
    {
        var items = _manager.PreviewTop();
        var result = new List<object>(items.Count);
        foreach (var (name, id, score) in items)
        {
            result.Add(new { name, id, score });
        }

        return Ok(result);
    }
}
