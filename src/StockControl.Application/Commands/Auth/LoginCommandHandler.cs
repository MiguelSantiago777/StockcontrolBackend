using MediatR;
using StockControl.Application.DTOs;
using StockControl.Application.Interfaces;
using StockControl.Domain.Common;
using StockControl.Domain.Interfaces;
using StockControl.Domain.Repositories;
using StockControl.Domain.ValueObjects;

namespace StockControl.Application.Commands.Auth;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    private static readonly Error CredenciaisInvalidas =
        Error.Unauthorized("Auth.CredenciaisInvalidas", "E-mail ou senha incorretos.");

    public LoginCommandHandler(
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<AuthResponseDto>(CredenciaisInvalidas);
        }

        var usuario = await _usuarioRepository.ObterPorEmailAsync(emailResult.Value, cancellationToken);
        if (usuario is null || !_passwordHasher.Verify(request.Password, usuario.Senha.Value))
        {
            return Result.Failure<AuthResponseDto>(CredenciaisInvalidas);
        }

        var accessToken = _jwtService.GerarAccessToken(usuario);
        var refreshToken = _jwtService.GerarRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        usuario.DefinirRefreshToken(refreshToken, refreshTokenExpiresAt);
        _usuarioRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
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
