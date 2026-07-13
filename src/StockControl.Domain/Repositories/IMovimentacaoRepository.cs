using StockControl.Domain.Aggregates;

namespace StockControl.Domain.Repositories;

public interface IMovimentacaoRepository : IRepository<Movimentacao>
{
    Task<IReadOnlyList<Movimentacao>> ObterPorProdutoAsync(Guid produtoId, CancellationToken cancellationToken = default);
}
