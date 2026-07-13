using MediatR;
using StockControl.Domain.Common;

namespace StockControl.Application.Commands.Auth;

public sealed class ChangePasswordCommand : IRequest<Result>
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
