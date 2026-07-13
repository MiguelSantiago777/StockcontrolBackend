using Microsoft.AspNetCore.SignalR;
using StockControl.Application.Interfaces;

namespace StockControl.Infrastructure.SignalR;

public sealed class SignalRNotificacaoService : INotificacaoService
{
    private readonly IHubContext<StockHub> _hubContext;

    public SignalRNotificacaoService(IHubContext<StockHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotificarDashboardAsync(object payload, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group(StockHub.Grupos.Dashboard)
            .SendAsync("DashboardAtualizado", payload, cancellationToken);
    }

    public Task NotificarMovimentacaoAsync(object payload, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group(StockHub.Grupos.Movimentacoes)
            .SendAsync("NovaMovimentacao", payload, cancellationToken);
    }

    public Task NotificarPosicaoEntregadorAsync(
        Guid entregadorId,
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group(StockHub.Grupos.Entregadores)
            .SendAsync("PosicaoEntregador", new { entregadorId, latitude, longitude }, cancellationToken);
    }
}
