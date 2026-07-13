using MediatR;
using StockControl.Application.Interfaces;
using StockControl.Domain.Common;
using StockControl.Domain.Interfaces;
using StockControl.Domain.Repositories;

namespace StockControl.Application.Commands.Auth;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public LogoutCommandHandler(
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Result.Success();
        }

        var usuario = await _usuarioRepository.GetByIdAsync(userId, cancellationToken);
        if (usuario is not null)
        {
            usuario.RevogarRefreshToken();
            _usuarioRepository.Update(usuario);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
