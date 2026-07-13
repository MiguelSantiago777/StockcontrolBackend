using MediatR;
using StockControl.Domain.Events;

namespace StockControl.Application.Events;

/// <summary>
/// Envelope que permite publicar Domain Events via MediatR
/// sem que a camada Domain conheça o MediatR.
/// </summary>
public sealed class DomainEventNotification<TDomainEvent> : INotification
    where TDomainEvent : IDomainEvent
{
    public DomainEventNotification(TDomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }

    public TDomainEvent DomainEvent { get; }
}
