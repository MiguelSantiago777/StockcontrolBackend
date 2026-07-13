using StockControl.Domain.Aggregates;
using StockControl.Domain.ValueObjects;

namespace StockControl.Domain.Repositories;

public interface IProdutoRepository : IRepository<Produto>
{
    Task<Produto?> ObterPorCodigoAsync(CodigoProduto codigo, CancellationToken cancellationToken = default);
    Task<bool> CodigoExisteAsync(CodigoProduto codigo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Produto>> ObterAbaixoDoEstoqueMinimoAsync(CancellationToken cancellationToken = default);
}
