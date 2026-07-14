using MediatR;
using StockControl.Application.Common;
using StockControl.Application.DTOs;
using StockControl.Application.Interfaces;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Common;
using StockControl.Domain.Enums;
using StockControl.Domain.Repositories;

namespace StockControl.Application.Queries.Dashboard;

internal static class MesesLabel
{
    private static readonly string[] Nomes =
        ["Jan", "Fev", "Mar", "Abr", "Mai", "Jun", "Jul", "Ago", "Set", "Out", "Nov", "Dez"];

    /// <summary>Últimos <paramref name="quantidade"/> meses (incluindo o atual), do mais antigo ao mais recente.</summary>
    public static List<(string Label, int Ano, int Mes)> UltimosMeses(int quantidade)
    {
        var hoje = DateTime.UtcNow;
        var meses = new List<(string, int, int)>();

        for (var i = quantidade - 1; i >= 0; i--)
        {
            var data = hoje.AddMonths(-i);
            meses.Add((Nomes[data.Month - 1], data.Year, data.Month));
        }

        return meses;
    }
}

// ─── Resumo ──────────────────────────────────────────────────────────────────

public sealed class ObterResumoDashboardQuery : IRequest<Result<DashboardSummaryDto>>
{
}

public sealed class ObterResumoDashboardQueryHandler : IRequestHandler<ObterResumoDashboardQuery, Result<DashboardSummaryDto>>
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<Pedido> _pedidoRepository;
    private readonly IRepository<Usuario> _usuarioRepository;
    private readonly IRepository<Movimentacao> _movimentacaoRepository;
    private readonly ICacheService _cache;

    public ObterResumoDashboardQueryHandler(
        IProdutoRepository produtoRepository,
        IRepository<Cliente> clienteRepository,
        IRepository<Pedido> pedidoRepository,
        IRepository<Usuario> usuarioRepository,
        IRepository<Movimentacao> movimentacaoRepository,
        ICacheService cache)
    {
        _produtoRepository = produtoRepository;
        _clienteRepository = clienteRepository;
        _pedidoRepository = pedidoRepository;
        _usuarioRepository = usuarioRepository;
        _movimentacaoRepository = movimentacaoRepository;
        _cache = cache;
    }

    public async Task<Result<DashboardSummaryDto>> Handle(ObterResumoDashboardQuery request, CancellationToken cancellationToken)
    {
        var cached = await _cache.GetAsync<DashboardSummaryDto>(CacheKeys.Dashboard, cancellationToken);
        if (cached is not null)
        {
            return Result.Success(cached);
        }

        var produtos = await _produtoRepository.ListAsync(cancellationToken);
        var abaixoDoMinimo = await _produtoRepository.ObterAbaixoDoEstoqueMinimoAsync(cancellationToken);
        var clientes = await _clienteRepository.ListAsync(cancellationToken);
        var pedidos = await _pedidoRepository.ListAsync(cancellationToken);
        var usuarios = await _usuarioRepository.ListAsync(cancellationToken);
        var movimentacoes = await _movimentacaoRepository.ListAsync(cancellationToken);

        var resumo = new DashboardSummaryDto
        {
            TotalProducts = produtos.Count,
            LowStockProducts = abaixoDoMinimo.Count(p => p.Estoque.Value > 0),
            OutOfStockProducts = produtos.Count(p => p.Estoque.Value == 0),
            TotalCustomers = clientes.Count,
            TotalOrders = pedidos.Count,
            ActiveDeliveries = pedidos.Count(p => p.Status == StatusPedido.EmEntrega),
            TotalUsers = usuarios.Count,
            TotalMovements = movimentacoes.Count
        };

        await _cache.SetAsync(CacheKeys.Dashboard, resumo, TimeSpan.FromMinutes(5), cancellationToken);
        return Result.Success(resumo);
    }
}

// ─── Pedidos por mês ─────────────────────────────────────────────────────────

public sealed class ObterPedidosPorMesQuery : IRequest<Result<MonthlySeriesDto>>
{
}

public sealed class ObterPedidosPorMesQueryHandler : IRequestHandler<ObterPedidosPorMesQuery, Result<MonthlySeriesDto>>
{
    private readonly IRepository<Pedido> _pedidoRepository;

    public ObterPedidosPorMesQueryHandler(IRepository<Pedido> pedidoRepository)
    {
        _pedidoRepository = pedidoRepository;
    }

    public async Task<Result<MonthlySeriesDto>> Handle(ObterPedidosPorMesQuery request, CancellationToken cancellationToken)
    {
        var pedidos = await _pedidoRepository.ListAsync(cancellationToken);
        var meses = MesesLabel.UltimosMeses(6);

        var valores = meses
            .Select(m => pedidos.Count(p => p.CreatedAt.Year == m.Ano && p.CreatedAt.Month == m.Mes))
            .ToList();

        return Result.Success(new MonthlySeriesDto
        {
            Labels = meses.Select(m => m.Label).ToList(),
            Datasets = [new DatasetDto { Label = "Pedidos", Values = valores }]
        });
    }
}

// ─── Entradas e saídas por mês ───────────────────────────────────────────────

public sealed class ObterMovimentacoesPorMesQuery : IRequest<Result<MonthlySeriesDto>>
{
}

public sealed class ObterMovimentacoesPorMesQueryHandler : IRequestHandler<ObterMovimentacoesPorMesQuery, Result<MonthlySeriesDto>>
{
    private readonly IRepository<Movimentacao> _movimentacaoRepository;

    public ObterMovimentacoesPorMesQueryHandler(IRepository<Movimentacao> movimentacaoRepository)
    {
        _movimentacaoRepository = movimentacaoRepository;
    }

    public async Task<Result<MonthlySeriesDto>> Handle(ObterMovimentacoesPorMesQuery request, CancellationToken cancellationToken)
    {
        var movimentacoes = await _movimentacaoRepository.ListAsync(cancellationToken);
        var meses = MesesLabel.UltimosMeses(6);

        var entradas = meses
            .Select(m => movimentacoes.Count(mv =>
                mv.Tipo == TipoMovimentacao.Entrada && mv.CreatedAt.Year == m.Ano && mv.CreatedAt.Month == m.Mes))
            .ToList();

        var saidas = meses
            .Select(m => movimentacoes.Count(mv =>
                mv.Tipo == TipoMovimentacao.Saida && mv.CreatedAt.Year == m.Ano && mv.CreatedAt.Month == m.Mes))
            .ToList();

        return Result.Success(new MonthlySeriesDto
        {
            Labels = meses.Select(m => m.Label).ToList(),
            Datasets =
            [
                new DatasetDto { Label = "Entradas", Values = entradas },
                new DatasetDto { Label = "Saídas", Values = saidas }
            ]
        });
    }
}

// ─── Produtos mais vendidos ──────────────────────────────────────────────────

public sealed class ObterProdutosMaisVendidosQuery : IRequest<Result<TopProductsDto>>
{
}

public sealed class ObterProdutosMaisVendidosQueryHandler : IRequestHandler<ObterProdutosMaisVendidosQuery, Result<TopProductsDto>>
{
    private readonly IPedidoRepository _pedidoRepository;

    public ObterProdutosMaisVendidosQueryHandler(IPedidoRepository pedidoRepository)
    {
        _pedidoRepository = pedidoRepository;
    }

    public async Task<Result<TopProductsDto>> Handle(ObterProdutosMaisVendidosQuery request, CancellationToken cancellationToken)
    {
        var pedidos = await _pedidoRepository.ObterTodosComItensAsync(cancellationToken);

        var maisVendidos = pedidos
            .Where(p => p.Status != StatusPedido.Cancelado)
            .SelectMany(p => p.Itens)
            .GroupBy(i => i.NomeProduto)
            .Select(g => new { Nome = g.Key, Quantidade = g.Sum(i => i.Quantidade.Value) })
            .OrderByDescending(x => x.Quantidade)
            .Take(5)
            .ToList();

        return Result.Success(new TopProductsDto
        {
            Labels = maisVendidos.Select(x => x.Nome).ToList(),
            Values = maisVendidos.Select(x => x.Quantidade).ToList()
        });
    }
}
