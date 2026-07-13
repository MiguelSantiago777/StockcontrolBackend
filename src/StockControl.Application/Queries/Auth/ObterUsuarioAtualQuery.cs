using MediatR;
using StockControl.Application.DTOs;
using StockControl.Domain.Common;

namespace StockControl.Application.Queries.Auth;

public sealed class ObterUsuarioAtualQuery : IRequest<Result<AuthUserDto>>
{
}
