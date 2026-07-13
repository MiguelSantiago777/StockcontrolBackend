using StockControl.Domain.Aggregates;
using StockControl.Domain.ValueObjects;

namespace StockControl.Domain.Repositories;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> ObterPorEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<bool> EmailExisteAsync(Email email, Guid? ignorarId = null, CancellationToken cancellationToken = default);
    Task<Usuario?> ObterPorRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
