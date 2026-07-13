using FluentAssertions;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Events;

namespace StockControl.UnitTests.Domain;

public class ProdutoTests
{
    private static Produto CriarProdutoValido(int estoque = 10) =>
        Produto.Criar("Notebook Dell", "Notebook i7", "NB-001", "7891234567895",
            4500.00m, estoque, 5, Guid.NewGuid()).Value;

    [Fact]
    public void Criar_ComDadosValidos_DeveRetornarSucessoEDispararEvento()
    {
        var result = Produto.Criar("Notebook", null, "NB-001", null, 100m, 10, 2, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.DomainEvents.Should().ContainSingle(e => e is ProdutoCriadoEvent);
    }

    [Fact]
    public void Criar_SemNome_DeveFalhar()
    {
        var result = Produto.Criar("", null, "NB-001", null, 100m, 10, 2, Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Produto.Nome");
    }

    [Fact]
    public void RemoverEstoque_QuantidadeMaiorQueDisponivel_DeveFalharComConflito()
    {
        var produto = CriarProdutoValido(estoque: 5);

        var result = produto.RemoverEstoque(10);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Produto.EstoqueInsuficiente");
        produto.Estoque.Value.Should().Be(5);
    }

    [Fact]
    public void RemoverEstoque_ZerandoEstoque_DeveDispararProdutoSemEstoqueEvent()
    {
        var produto = CriarProdutoValido(estoque: 3);
        produto.ClearDomainEvents();

        produto.RemoverEstoque(3);

        produto.DomainEvents.Should().ContainSingle(e => e is ProdutoSemEstoqueEvent);
    }
}
