using MediatR;
using StockControl.Application.Interfaces;
using StockControl.Domain.Common;
using StockControl.Domain.Interfaces;
using StockControl.Domain.Repositories;
using StockControl.Domain.ValueObjects;

namespace StockControl.Application.Commands.Auth;

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUser;

    public ChangePasswordCommandHandler(
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUser)
    {
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Result.Failure(Error.Unauthorized("Auth.NaoAutenticado", "Usuário não autenticado."));
        }

        var usuario = await _usuarioRepository.GetByIdAsync(userId, cancellationToken);
        if (usuario is null)
        {
            return Result.Failure(Error.Unauthorized("Auth.NaoAutenticado", "Usuário não autenticado."));
        }

        if (!_passwordHasher.Verify(request.CurrentPassword, usuario.Senha.Value))
        {
            return Result.Failure(Error.Validation("Auth.SenhaAtualIncorreta", "A senha atual está incorreta."));
        }

        var novoHashResult = SenhaHash.Create(_passwordHasher.Hash(request.NewPassword));
        if (novoHashResult.IsFailure)
        {
            return Result.Failure(novoHashResult.Error);
        }

        usuario.AlterarSenha(novoHashResult.Value);
        usuario.RevogarRefreshToken();
        _usuarioRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
