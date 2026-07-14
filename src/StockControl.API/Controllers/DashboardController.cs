using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockControl.API.Extensions;
using StockControl.Application.DTOs;
using StockControl.Application.Queries.Dashboard;
using static StockControl.Infrastructure.DependencyInjection;

namespace StockControl.API.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize(Policy = Policies.Leitura)]
public sealed class DashboardController : ControllerBase
{
    private readonly ISender _sender;

    public DashboardController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult> Resumo(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ObterResumoDashboardQuery(), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("orders-by-month")]
    [ProducesResponseType(typeof(MonthlySeriesDto), StatusCodes.Status200OK)]
    public async Task<ActionResult> PedidosPorMes(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ObterPedidosPorMesQuery(), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("stock-movements")]
    [ProducesResponseType(typeof(MonthlySeriesDto), StatusCodes.Status200OK)]
    public async Task<ActionResult> MovimentacoesPorMes(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ObterMovimentacoesPorMesQuery(), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("top-products")]
    [ProducesResponseType(typeof(TopProductsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult> ProdutosMaisVendidos(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ObterProdutosMaisVendidosQuery(), cancellationToken);
        return result.ToActionResult(this);
    }
}
