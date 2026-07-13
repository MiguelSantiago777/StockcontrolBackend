using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockControl.API.Extensions;
using StockControl.Application.Commands.Auth;
using StockControl.Application.DTOs;
using StockControl.Application.Queries.Auth;

namespace StockControl.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Authorize]
public sealed class AuthController : ControllerBase
{
    private const string RefreshCookieName = "sc_refresh";

    private readonly ISender _sender;
    private readonly IWebHostEnvironment _environment;

    public AuthController(ISender sender, IWebHostEnvironment environment)
    {
        _sender = sender;
        _environment = environment;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess)
        {
            SetRefreshCookie(result.Value.RefreshToken, result.Value.RefreshTokenExpiresAt);
        }
        return result.ToActionResult(this);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess)
        {
            SetRefreshCookie(result.Value.RefreshToken, result.Value.RefreshTokenExpiresAt);
        }
        return result.ToActionResult(this);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Logout(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new LogoutCommand(), cancellationToken);
        DeleteRefreshCookie();
        return result.ToActionResult(this);
    }

    private void SetRefreshCookie(string refreshToken, DateTime expiresAtUtc)
    {
        Response.Cookies.Append(RefreshCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(),
            SameSite = _environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
            Expires = expiresAtUtc,
            Path = "/"
        });
    }

    private void DeleteRefreshCookie()
    {
        Response.Cookies.Delete(RefreshCookieName, new CookieOptions { Path = "/" });
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(AuthUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ObterUsuarioAtualQuery(), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Stub: não envia e-mail (nenhum serviço de e-mail configurado no projeto ainda).
    /// Sempre retorna 204 — por segurança, nunca revela se o e-mail existe ou não na base.
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        return NoContent();
    }
}

public sealed class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}
