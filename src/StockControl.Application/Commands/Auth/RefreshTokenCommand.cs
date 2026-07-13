using MediatR;
using StockControl.Application.DTOs;
using StockControl.Domain.Common;

namespace StockControl.Application.Commands.Auth;

public sealed class RefreshTokenCommand : IRequest<Result<AuthResponseDto>>
{
    public string RefreshToken { get; set; } = string.Empty;
}
