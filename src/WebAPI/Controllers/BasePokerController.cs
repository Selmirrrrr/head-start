using Microsoft.AspNetCore.Mvc;

namespace HeadStart.WebAPI.Controllers;

/// <summary>
/// Base controller.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BasePokerController : ControllerBase
{
}