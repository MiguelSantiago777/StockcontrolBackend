using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockControl.API.Extensions;
using StockControl.Application.Commands.Pedidos;
using StockControl.Application.DTOs;
using StockControl.Application.Queries.Pedidos;
using static StockControl.Infrastructure.DependencyInjection;

namespace StockControl.API.Controllers;

[ApiController]
[Route("api/v1/orders")]
[Authorize]
public sealed class PedidosController : ControllerBase
{
    private readonly ISender _sender;

    public PedidosController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Leitura)]
    [ProducesResponseType(typeof(PagedResultDto<PedidoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListarPedidosQuery(page, pageSize, status), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.Leitura)]
    [ProducesResponseType(typeof(PedidoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ObterPedidoPorIdQuery(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [Authorize(Policy = Policies.GerenciaEstoque)]
    [ProducesResponseType(typeof(PedidoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Criar([FromBody] CriarPedidoCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.ToCreatedResult(this, nameof(ObterPorId), dto => new { id = dto.Id });
    }

    [HttpPatch("{id:guid}/start-delivery")]
    [Authorize(Policy = Policies.Entregas)]
    [ProducesResponseType(typeof(PedidoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> IniciarEntrega(Guid id, [FromBody] IniciarEntregaPedidoCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPatch("{id:guid}/finish-delivery")]
    [Authorize(Policy = Policies.Entregas)]
    [ProducesResponseType(typeof(PedidoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> FinalizarEntrega(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new FinalizarEntregaPedidoCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPatch("{id:guid}/cancel")]
    [Authorize(Policy = Policies.GerenciaEstoque)]
    [ProducesResponseType(typeof(PedidoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Cancelar(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CancelarPedidoCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }
}
