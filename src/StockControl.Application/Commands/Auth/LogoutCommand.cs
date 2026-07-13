using MediatR;
using StockControl.Domain.Common;

namespace StockControl.Application.Commands.Auth;

public sealed class LogoutCommand : IRequest<Result>
{
}
