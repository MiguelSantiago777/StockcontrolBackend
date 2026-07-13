using StockControl.Domain.Common;
using StockControl.Domain.Entities;
using StockControl.Domain.Enums;
using StockControl.Domain.Events;
using StockControl.Domain.ValueObjects;

namespace StockControl.Domain.Aggregates;

public sealed class Pedido : AggregateRoot
{
    private readonly List<ItemPedido> _itens = [];

    private Pedido() { } // EF Core

    private Pedido(Guid clienteId, Endereco enderecoEntrega)
    {
        ClienteId = clienteId;
        EnderecoEntrega = enderecoEntrega;
        Status = StatusPedido.Criado;
    }

    public Guid ClienteId { get; private set; }
    public Guid? EntregadorId { get; private set; }
    public StatusPedido Status { get; private set; }
    public Endereco EnderecoEntrega { get; private set; } = null!;
    public DateTime? IniciadoEntregaEm { get; private set; }
    public DateTime? FinalizadoEm { get; private set; }

    public IReadOnlyCollection<ItemPedido> Itens => _itens.AsReadOnly();

    public Money Total => _itens.Aggregate(Money.Zero,
        (acc, item) => acc.Add(item.PrecoUnitario.Multiply(item.Quantidade.Value)));

    public static Result<Pedido> Criar(Guid clienteId, Endereco enderecoEntrega)
    {
        if (clienteId == Guid.Empty)
            return Result.Failure<Pedido>(Error.Validation("Pedido.Cliente", "Cliente é obrigatório."));

        var pedido = new Pedido(clienteId, enderecoEntrega);
        pedido.RaiseDomainEvent(new PedidoCriadoEvent(pedido.Id, clienteId));
        return pedido;
    }

    public Result AdicionarItem(Guid produtoId, string nomeProduto, decimal precoUnitario, int quantidade)
    {
        if (Status != StatusPedido.Criado)
            return Result.Failure(Error.Conflict("Pedido.Fechado", "Não é possível alterar um pedido já processado."));

        var preco = Money.Create(precoUnitario);
        if (preco.IsFailure) return Result.Failure(preco.Error);

        var qtd = Quantidade.Create(quantidade);
        if (qtd.IsFailure) return Result.Failure(qtd.Error);
        if (qtd.Value.Value == 0)
            return Result.Failure(Error.Validation("Pedido.ItemQuantidade", "Quantidade deve ser maior que zero."));

        _itens.Add(ItemPedido.Criar(Id, produtoId, nomeProduto, preco.Value, qtd.Value));
        MarkAsUpdated();
        return Result.Success();
    }

    public Result IniciarEntrega(Guid entregadorId)
    {
        if (Status is not (StatusPedido.AguardandoEntrega or StatusPedido.EmSeparacao or StatusPedido.Criado))
            return Result.Failure(Error.Conflict("Pedido.Status", "O pedido não está apto para entrega."));

        EntregadorId = entregadorId;
        Status = StatusPedido.EmEntrega;
        IniciadoEntregaEm = DateTime.UtcNow;
        MarkAsUpdated();
        RaiseDomainEvent(new EntregaIniciadaEvent(Id, entregadorId));
        return Result.Success();
    }

    public Result FinalizarEntrega()
    {
        if (Status != StatusPedido.EmEntrega || EntregadorId is null)
            return Result.Failure(Error.Conflict("Pedido.Status", "O pedido não está em entrega."));

        Status = StatusPedido.Finalizado;
        FinalizadoEm = DateTime.UtcNow;
        MarkAsUpdated();
        RaiseDomainEvent(new EntregaFinalizadaEvent(Id, EntregadorId.Value));
        RaiseDomainEvent(new PedidoFinalizadoEvent(Id));
        return Result.Success();
    }

    public Result Cancelar()
    {
        if (Status is StatusPedido.Finalizado or StatusPedido.Cancelado)
            return Result.Failure(Error.Conflict("Pedido.Status", "O pedido não pode ser cancelado."));

        Status = StatusPedido.Cancelado;
        MarkAsUpdated();
        return Result.Success();
    }
}
