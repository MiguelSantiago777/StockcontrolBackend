using MediatR;
using Microsoft.Extensions.Logging;
using StockControl.Application.Interfaces;
using StockControl.Domain.Events;

namespace StockControl.Application.Events;

public sealed class ProdutoSemEstoqueEventHandler
    : INotificationHandler<DomainEventNotification<ProdutoSemEstoqueEvent>>
{
    private readonly INotificacaoService _notificacaoService;
    private readonly ILogger<ProdutoSemEstoqueEventHandler> _logger;

    public ProdutoSemEstoqueEventHandler(
        INotificacaoService notificacaoService,
        ILogger<ProdutoSemEstoqueEventHandler> logger)
    {
        _notificacaoService = notificacaoService;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<ProdutoSemEstoqueEvent> notification,
        CancellationToken cancellationToken)
    {
        var evento = notification.DomainEvent;

        _logger.LogWarning("Produto sem estoque: {ProdutoId} - {Nome}", evento.ProdutoId, evento.Nome);

        await _notificacaoService.NotificarDashboardAsync(
            new { tipo = "ProdutoSemEstoque", evento.ProdutoId, evento.Nome },
            cancellationToken);
    }
}
