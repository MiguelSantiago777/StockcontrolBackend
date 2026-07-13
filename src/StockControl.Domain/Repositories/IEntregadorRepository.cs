using StockControl.Domain.Aggregates;
using StockControl.Domain.Enums;
using StockControl.Domain.ValueObjects;

namespace StockControl.Domain.Repositories;

public interface IEntregadorRepository : IRepository<Entregador>
{
    Task<IReadOnlyList<Entregador>> ObterPorStatusAsync(StatusEntregador status, CancellationToken cancellationToken = default);
    Task<bool> CpfExisteAsync(Cpf cpf, Guid? ignorarId = null, CancellationToken cancellationToken = default);
    Task<bool> UsuarioJaVinculadoAsync(Guid usuarioId, Guid? ignorarId = null, CancellationToken cancellationToken = default);
}
