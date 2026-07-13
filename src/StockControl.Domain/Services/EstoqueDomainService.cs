using StockControl.Domain.Aggregates;
using StockControl.Domain.Common;
using StockControl.Domain.Enums;

namespace StockControl.Domain.Services;

/// <summary>
/// Domain Service: orquestra regras que envolvem mais de um agregado
/// (Produto + Movimentacao), sem depender de infraestrutura.
/// </summary>
public sealed class EstoqueDomainService
{
    public Result<Movimentacao> Movimentar(Produto produto, TipoMovimentacao tipo,
        int quantidade, Guid usuarioId, string? observacao = null)
    {
        var resultadoEstoque = tipo switch
        {
            TipoMovimentacao.Entrada or TipoMovimentacao.Devolucao => produto.AdicionarEstoque(quantidade),
            TipoMovimentacao.Saida or TipoMovimentacao.Perda => produto.RemoverEstoque(quantidade),
            TipoMovimentacao.Ajuste => produto.AdicionarEstoque(quantidade),
            _ => Result.Failure(Error.Validation("Movimentacao.Tipo", "Tipo de movimentação inválido."))
        };

        if (resultadoEstoque.IsFailure)
            return Result.Failure<Movimentacao>(resultadoEstoque.Error);

        return Movimentacao.Criar(produto.Id, tipo, quantidade, usuarioId, observacao);
    }
}
