namespace StockControl.Application.Interfaces;

/// <summary>Abstração do SignalR — a Application não conhece o hub.</summary>
public interface INotificacaoService
{
    Task NotificarDashboardAsync(object payload, CancellationToken cancellationToken = default);
    Task NotificarMovimentacaoAsync(object payload, CancellationToken cancellationToken = default);
    Task NotificarPosicaoEntregadorAsync(Guid entregadorId, double latitude, double longitude, CancellationToken cancellationToken = default);
}
