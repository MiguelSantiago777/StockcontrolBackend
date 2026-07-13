using StockControl.Domain.Common;
using StockControl.Domain.Events;
using StockControl.Domain.ValueObjects;

namespace StockControl.Domain.Aggregates;

public sealed class Produto : AggregateRoot
{
    private Produto() { } // EF Core

    private Produto(string nome, string? descricao, CodigoProduto codigo,
        CodigoBarras? codigoBarras, Money preco, Quantidade estoque,
        Quantidade estoqueMinimo, Guid categoriaId, Guid? fornecedorId)
    {
        Nome = nome;
        Descricao = descricao;
        Codigo = codigo;
        CodigoBarras = codigoBarras;
        Preco = preco;
        Estoque = estoque;
        EstoqueMinimo = estoqueMinimo;
        CategoriaId = categoriaId;
        FornecedorId = fornecedorId;
    }

    public string Nome { get; private set; } = null!;
    public string? Descricao { get; private set; }
    public CodigoProduto Codigo { get; private set; } = null!;
    public CodigoBarras? CodigoBarras { get; private set; }
    public Money Preco { get; private set; } = null!;
    public Quantidade Estoque { get; private set; } = null!;
    public Quantidade EstoqueMinimo { get; private set; } = null!;
    public Guid CategoriaId { get; private set; }
    public Guid? FornecedorId { get; private set; }
    public string? ImagemUrl { get; private set; }

    /// <summary>Factory Method — único ponto de criação do agregado.</summary>
    public static Result<Produto> Criar(string? nome, string? descricao, string? codigo,
        string? codigoBarras, decimal preco, int estoqueInicial, int estoqueMinimo,
        Guid categoriaId, Guid? fornecedorId = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Result.Failure<Produto>(Error.Validation("Produto.Nome", "O nome do produto é obrigatório."));

        var codigoResult = CodigoProduto.Create(codigo);
        if (codigoResult.IsFailure) return Result.Failure<Produto>(codigoResult.Error);

        Result<CodigoBarras>? barrasResult = null;
        if (!string.IsNullOrWhiteSpace(codigoBarras))
        {
            barrasResult = ValueObjects.CodigoBarras.Create(codigoBarras);
            if (barrasResult.IsFailure) return Result.Failure<Produto>(barrasResult.Error);
        }

        var precoResult = Money.Create(preco);
        if (precoResult.IsFailure) return Result.Failure<Produto>(precoResult.Error);

        var estoqueResult = Quantidade.Create(estoqueInicial);
        if (estoqueResult.IsFailure) return Result.Failure<Produto>(estoqueResult.Error);

        var minimoResult = Quantidade.Create(estoqueMinimo);
        if (minimoResult.IsFailure) return Result.Failure<Produto>(minimoResult.Error);

        var produto = new Produto(nome.Trim(), descricao?.Trim(), codigoResult.Value,
            barrasResult?.Value, precoResult.Value, estoqueResult.Value,
            minimoResult.Value, categoriaId, fornecedorId);

        produto.RaiseDomainEvent(new ProdutoCriadoEvent(produto.Id, produto.Nome));
        return produto;
    }

    public Result AdicionarEstoque(int quantidade)
    {
        var qtd = Quantidade.Create(quantidade);
        if (qtd.IsFailure) return Result.Failure(qtd.Error);

        var novo = Estoque.Somar(qtd.Value);
        if (novo.IsFailure) return Result.Failure(novo.Error);

        Estoque = novo.Value;
        MarkAsUpdated();
        RaiseDomainEvent(new ProdutoAtualizadoEvent(Id));
        return Result.Success();
    }

    public Result RemoverEstoque(int quantidade)
    {
        var qtd = Quantidade.Create(quantidade);
        if (qtd.IsFailure) return Result.Failure(qtd.Error);

        if (qtd.Value.Value > Estoque.Value)
            return Result.Failure(Error.Conflict("Produto.EstoqueInsuficiente",
                $"Estoque insuficiente. Disponível: {Estoque.Value}."));

        Estoque = Estoque.Subtrair(qtd.Value).Value;
        MarkAsUpdated();

        if (Estoque.Value == 0)
            RaiseDomainEvent(new ProdutoSemEstoqueEvent(Id, Nome));

        return Result.Success();
    }

    public Result AtualizarPreco(decimal novoPreco)
    {
        var preco = Money.Create(novoPreco);
        if (preco.IsFailure) return Result.Failure(preco.Error);

        Preco = preco.Value;
        MarkAsUpdated();
        RaiseDomainEvent(new ProdutoAtualizadoEvent(Id));
        return Result.Success();
    }

    public bool EstoqueAbaixoDoMinimo() => Estoque.Value < EstoqueMinimo.Value;

    /// <summary>
    /// Atualiza todos os dados editáveis do produto (usado pela edição via formulário).
    /// Sobrescreve o estoque diretamente — para ajustes rastreados via movimentação,
    /// prefira AdicionarEstoque/RemoverEstoque.
    /// </summary>
    public Result AtualizarDados(string? nome, string? descricao, string? codigo,
        string? codigoBarras, decimal preco, int estoque, int estoqueMinimo,
        Guid categoriaId, Guid? fornecedorId)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Result.Failure(Error.Validation("Produto.Nome", "O nome do produto é obrigatório."));

        var codigoResult = CodigoProduto.Create(codigo);
        if (codigoResult.IsFailure) return Result.Failure(codigoResult.Error);

        ValueObjects.CodigoBarras? novoCodigoBarras = null;
        if (!string.IsNullOrWhiteSpace(codigoBarras))
        {
            var barrasResult = ValueObjects.CodigoBarras.Create(codigoBarras);
            if (barrasResult.IsFailure) return Result.Failure(barrasResult.Error);
            novoCodigoBarras = barrasResult.Value;
        }

        var precoResult = Money.Create(preco);
        if (precoResult.IsFailure) return Result.Failure(precoResult.Error);

        var estoqueResult = Quantidade.Create(estoque);
        if (estoqueResult.IsFailure) return Result.Failure(estoqueResult.Error);

        var minimoResult = Quantidade.Create(estoqueMinimo);
        if (minimoResult.IsFailure) return Result.Failure(minimoResult.Error);

        Nome = nome.Trim();
        Descricao = descricao?.Trim();
        Codigo = codigoResult.Value;
        CodigoBarras = novoCodigoBarras;
        Preco = precoResult.Value;
        Estoque = estoqueResult.Value;
        EstoqueMinimo = minimoResult.Value;
        CategoriaId = categoriaId;
        FornecedorId = fornecedorId;

        MarkAsUpdated();
        RaiseDomainEvent(new ProdutoAtualizadoEvent(Id));

        if (Estoque.Value == 0)
            RaiseDomainEvent(new ProdutoSemEstoqueEvent(Id, Nome));

        return Result.Success();
    }

    public void DefinirImagem(string url)
    {
        ImagemUrl = url;
        MarkAsUpdated();
    }
}
