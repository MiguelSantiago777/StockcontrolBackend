using MediatR;
using StockControl.Application.DTOs;
using StockControl.Application.Interfaces;
using StockControl.Domain.Common;
using StockControl.Domain.Repositories;

namespace StockControl.Application.Queries.Auth;

public sealed class ObterUsuarioAtualQueryHandler : IRequestHandler<ObterUsuarioAtualQuery, Result<AuthUserDto>>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ICurrentUserService _currentUser;

    private static readonly Error NaoAutenticado =
        Error.Unauthorized("Auth.NaoAutenticado", "Usuário não autenticado.");

    public ObterUsuarioAtualQueryHandler(IUsuarioRepository usuarioRepository, ICurrentUserService currentUser)
    {
        _usuarioRepository = usuarioRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<AuthUserDto>> Handle(ObterUsuarioAtualQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Result.Failure<AuthUserDto>(NaoAutenticado);
        }

        var usuario = await _usuarioRepository.GetByIdAsync(userId, cancellationToken);
        if (usuario is null)
        {
            return Result.Failure<AuthUserDto>(NaoAutenticado);
        }

        return new AuthUserDto
        {
            Id = usuario.Id,
            Name = usuario.Nome,
            Email = usuario.Email.Value,
            Role = usuario.Perfil.ToString()
        };
    }
}
