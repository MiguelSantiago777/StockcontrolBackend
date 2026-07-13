namespace StockControl.Domain.Events;

public sealed class PedidoCriadoEvent : DomainEvent
{
    public PedidoCriadoEvent(Guid pedidoId, Guid clienteId)
    {
        PedidoId = pedidoId;
        ClienteId = clienteId;
    }

    public Guid PedidoId { get; }
    public Guid ClienteId { get; }
}

public sealed class PedidoFinalizadoEvent : DomainEvent
{
    public PedidoFinalizadoEvent(Guid pedidoId)
    {
        PedidoId = pedidoId;
    }

    public Guid PedidoId { get; }
}

public sealed class EntregaIniciadaEvent : DomainEvent
{
    public EntregaIniciadaEvent(Guid pedidoId, Guid entregadorId)
    {
        PedidoId = pedidoId;
        EntregadorId = entregadorId;
    }

    public Guid PedidoId { get; }
    public Guid EntregadorId { get; }
}

public sealed class EntregaFinalizadaEvent : DomainEvent
{
    public EntregaFinalizadaEvent(Guid pedidoId, Guid entregadorId)
    {
        PedidoId = pedidoId;
        EntregadorId = entregadorId;
    }

    public Guid PedidoId { get; }
    public Guid EntregadorId { get; }
}
