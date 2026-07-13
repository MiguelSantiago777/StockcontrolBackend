using StockControl.Domain.Common;
using StockControl.Domain.Enums;
using StockControl.Domain.ValueObjects;

namespace StockControl.Domain.Aggregates;

public sealed class Movimentacao : AggregateRoot
{
    private Movimentacao() { } // EF Core

    private Movimentacao(Guid produtoId, TipoMovimentacao tipo, Quantidade quantidade,
        Guid usuarioId, string? observacao)
    {
        ProdutoId = produtoId;
        Tipo = tipo;
        Quantidade = quantidade;
        UsuarioId = usuarioId;
        Observacao = observacao;
    }

    public Guid ProdutoId { get; private set; }
    public TipoMovimentacao Tipo { get; private set; }
    public Quantidade Quantidade { get; private set; } = null!;
    public Guid UsuarioId { get; private set; }
    public string? Observacao { get; private set; }

    public static Result<Movimentacao> Criar(Guid produtoId, TipoMovimentacao tipo,
        int quantidade, Guid usuarioId, string? observacao = null)
    {
        if (produtoId == Guid.Empty)
            return Result.Failure<Movimentacao>(Error.Validation("Movimentacao.Produto", "Produto é obrigatório."));

        var qtd = Quantidade.Create(quantidade);
        if (qtd.IsFailure) return Result.Failure<Movimentacao>(qtd.Error);
        if (qtd.Value.Value == 0)
            return Result.Failure<Movimentacao>(Error.Validation("Movimentacao.Quantidade", "Quantidade deve ser maior que zero."));

        return new Movimentacao(produtoId, tipo, qtd.Value, usuarioId, observacao);
    }
}
