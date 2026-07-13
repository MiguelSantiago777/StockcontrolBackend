using StockControl.Domain.Common;
using StockControl.Domain.ValueObjects;

namespace StockControl.Domain.Entities;

/// <summary>Entidade filha do agregado Pedido — nunca acessada diretamente por repositório.</summary>
public sealed class ItemPedido : BaseEntity
{
    private ItemPedido() { } // EF Core

    private ItemPedido(Guid pedidoId, Guid produtoId, string nomeProduto,
        Money precoUnitario, Quantidade quantidade)
    {
        PedidoId = pedidoId;
        ProdutoId = produtoId;
        NomeProduto = nomeProduto;
        PrecoUnitario = precoUnitario;
        Quantidade = quantidade;
    }

    public Guid PedidoId { get; private set; }
    public Guid ProdutoId { get; private set; }
    public string NomeProduto { get; private set; } = null!;
    public Money PrecoUnitario { get; private set; } = null!;
    public Quantidade Quantidade { get; private set; } = null!;

    internal static ItemPedido Criar(Guid pedidoId, Guid produtoId, string nomeProduto,
        Money precoUnitario, Quantidade quantidade) =>
        new(pedidoId, produtoId, nomeProduto, precoUnitario, quantidade);
}
