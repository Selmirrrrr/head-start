using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace HeadStart.BFF.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class FirstLoginController : ControllerBase
{
    private readonly ILogger<FirstLoginController> _logger;
    private readonly IConfiguration _configuration;

    public FirstLoginController(
        ILogger<FirstLoginController> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet("status")]
    public IActionResult GetFirstLoginStatus()
    {
        // Check if the current user has a tenant
        // This will be implemented when we have the full user context
        return Ok(new { RequiresFirstLogin = false });
    }

    [HttpPost("complete")]
    public async Task<IActionResult> CompleteFirstLogin([FromBody] FirstLoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // TODO: Create tenant in database
        // TODO: Assign user as admin of the new tenant
        // TODO: Update user's last selected tenant

        var baseDomain = _configuration["Tenancy:BaseDomain"] ?? "headstart.ch";
        var redirectUrl = $"https://{request.TenantCode.ToLowerInvariant()}.{baseDomain}";

        return Ok(new FirstLoginResponse
        {
            Success = true,
            RedirectUrl = redirectUrl
        });
    }

    [HttpPost("validate-tenant-code")]
    public async Task<IActionResult> ValidateTenantCode([FromBody] ValidateTenantCodeRequest request)
    {
        // TODO: Check if tenant code is unique in database
        // For now, simulate validation
        var isValid = !string.IsNullOrWhiteSpace(request.TenantCode) &&
                      request.TenantCode.Length >= 3 &&
                      System.Text.RegularExpressions.Regex.IsMatch(request.TenantCode, @"^[a-zA-Z0-9-]+$");

        return Ok(new { IsValid = isValid });
    }
}

public class FirstLoginRequest
{
    [Required]
    [MaxLength(250)]
    public required string CompanyName { get; set; }

    [Required]
    [MaxLength(500)]
    public required string CompanyAddress { get; set; }

    [Required]
    [EmailAddress]
    public required string CompanyEmail { get; set; }

    [Required]
    public required string CompanySize { get; set; } // TPE, PME, Grande

    [Required]
    [MaxLength(250)]
    public required string BusinessDomain { get; set; }

    [Required]
    [RegularExpression(@"^[a-zA-Z0-9-]+$", ErrorMessage = "Tenant code can only contain letters, numbers and hyphens")]
    [MinLength(3)]
    [MaxLength(50)]
    public required string TenantCode { get; set; }
}

public class FirstLoginResponse
{
    public bool Success { get; set; }
    public string? RedirectUrl { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ValidateTenantCodeRequest
{
    [Required]
    public required string TenantCode { get; set; }
}