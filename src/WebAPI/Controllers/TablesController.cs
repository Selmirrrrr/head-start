using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeadStart.WebAPI.Controllers;

/// <summary>
/// Responsible for managing poker tables.
/// </summary>
[Authorize]
public sealed class TablesController : BasePokerController
{
    private const string IdRouteParam = "{id:guid}";

    /// <summary>
    /// Returns a table by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpGet(IdRouteParam)]
    public async Task<ActionResult> Get([FromRoute] Guid id, CancellationToken ct)
        => Ok(await Task.FromResult(id));
}
