using MediatR;
using StockControl.Application.DTOs;
using StockControl.Domain.Common;

namespace StockControl.Application.Commands.Auth;

public sealed class LoginCommand : IRequest<Result<AuthResponseDto>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
