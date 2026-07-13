using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockControl.API.Extensions;
using StockControl.Application.Commands.Entregadores;
using StockControl.Application.DTOs;
using StockControl.Application.Queries.Entregadores;
using static StockControl.Infrastructure.DependencyInjection;

namespace StockControl.API.Controllers;

[ApiController]
[Route("api/v1/drivers")]
[Authorize]
public sealed class EntregadoresController : ControllerBase
{
    private readonly ISender _sender;

    public EntregadoresController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Leitura)]
    [ProducesResponseType(typeof(PagedResultDto<EntregadorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListarEntregadoresQuery(page, pageSize, search), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("eligible-users")]
    [Authorize(Policy = Policies.GerenciaEstoque)]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioElegivelDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> UsuariosElegiveis(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListarUsuariosElegiveisParaEntregadorQuery(), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.Leitura)]
    [ProducesResponseType(typeof(EntregadorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ObterEntregadorPorIdQuery(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [Authorize(Policy = Policies.GerenciaEstoque)]
    [ProducesResponseType(typeof(EntregadorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Criar([FromBody] CriarEntregadorCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.ToCreatedResult(this, nameof(ObterPorId), dto => new { id = dto.Id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.GerenciaEstoque)]
    [ProducesResponseType(typeof(EntregadorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Atualizar(Guid id, [FromBody] AtualizarEntregadorCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = Policies.GerenciaEstoque)]
    [ProducesResponseType(typeof(EntregadorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AtualizarStatus(Guid id, [FromBody] AtualizarStatusEntregadorCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPatch("{id:guid}/position")]
    [Authorize(Policy = Policies.GerenciaEstoque)]
    [ProducesResponseType(typeof(EntregadorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AtualizarPosicao(Guid id, [FromBody] AtualizarPosicaoEntregadorCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.GerenciaEstoque)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ExcluirEntregadorCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }
}
