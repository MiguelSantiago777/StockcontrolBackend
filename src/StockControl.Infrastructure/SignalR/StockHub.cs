using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace StockControl.Infrastructure.SignalR;

[Authorize]
public sealed class StockHub : Hub
{
    public const string Route = "/hubs/stock";

    public async Task EntrarNoGrupoDashboard() =>
        await Groups.AddToGroupAsync(Context.ConnectionId, Grupos.Dashboard);

    public async Task EntrarNoGrupoEntregadores() =>
        await Groups.AddToGroupAsync(Context.ConnectionId, Grupos.Entregadores);

    public static class Grupos
    {
        public const string Dashboard = "dashboard";
        public const string Entregadores = "entregadores";
        public const string Movimentacoes = "movimentacoes";
    }
}
