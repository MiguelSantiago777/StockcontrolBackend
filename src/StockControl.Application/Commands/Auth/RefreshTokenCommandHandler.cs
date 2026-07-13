using MediatR;
using StockControl.Application.DTOs;
using StockControl.Application.Interfaces;
using StockControl.Domain.Common;
using StockControl.Domain.Interfaces;
using StockControl.Domain.Repositories;

namespace StockControl.Application.Commands.Auth;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;

    private static readonly Error TokenInvalido =
        Error.Unauthorized("Auth.RefreshTokenInvalido", "Refresh token inválido ou expirado.");

    public RefreshTokenCommandHandler(
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        IJwtService jwtService)
    {
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result.Failure<AuthResponseDto>(TokenInvalido);
        }

        var usuario = await _usuarioRepository.ObterPorRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (usuario is null || !usuario.RefreshTokenValido(request.RefreshToken))
        {
            return Result.Failure<AuthResponseDto>(TokenInvalido);
        }

        var novoAccessToken = _jwtService.GerarAccessToken(usuario);
        var novoRefreshToken = _jwtService.GerarRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        usuario.DefinirRefreshToken(novoRefreshToken, refreshTokenExpiresAt);
        _usuarioRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = novoAccessToken,
            RefreshToken = novoRefreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            User = new AuthUserDto
            {
                Id = usuario.Id,
                Name = usuario.Nome,
                Email = usuario.Email.Value,
                Role = usuario.Perfil.ToString()
            }
        };
    }
}
