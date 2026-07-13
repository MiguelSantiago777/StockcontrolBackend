namespace StockControl.Domain.Events;

public sealed class ProdutoCriadoEvent : DomainEvent
{
    public ProdutoCriadoEvent(Guid produtoId, string nome)
    {
        ProdutoId = produtoId;
        Nome = nome;
    }

    public Guid ProdutoId { get; }
    public string Nome { get; }
}

public sealed class ProdutoAtualizadoEvent : DomainEvent
{
    public ProdutoAtualizadoEvent(Guid produtoId)
    {
        ProdutoId = produtoId;
    }

    public Guid ProdutoId { get; }
}

public sealed class ProdutoSemEstoqueEvent : DomainEvent
{
    public ProdutoSemEstoqueEvent(Guid produtoId, string nome)
    {
        ProdutoId = produtoId;
        Nome = nome;
    }

    public Guid ProdutoId { get; }
    public string Nome { get; }
}
