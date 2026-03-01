using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosAPI.Application.DTOs.Request;
using TurnosAPI.Application.Interfaces;

namespace TurnosAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    [HttpPost("login-admin")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAdmin(
        [FromBody] AdminLoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.AdminLoginAsync(request, cancellationToken);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("login-client")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginClient(
        [FromBody] ClientLoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.ClientLoginAsync(request, cancellationToken);
        return Ok(result);
    }
}