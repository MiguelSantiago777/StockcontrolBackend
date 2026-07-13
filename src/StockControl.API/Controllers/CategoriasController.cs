using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockControl.API.Extensions;
using StockControl.Application.Commands.Categorias;
using StockControl.Application.DTOs;
using StockControl.Application.Queries.Categorias;
using static StockControl.Infrastructure.DependencyInjection;

namespace StockControl.API.Controllers;

[ApiController]
[Route("api/v1/categories")]
[Authorize]
public sealed class CategoriasController : ControllerBase
{
    private readonly ISender _sender;

    public CategoriasController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Leitura)]
    [ProducesResponseType(typeof(PagedResultDto<CategoriaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListarCategoriasQuery(page, pageSize, search), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.Leitura)]
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ObterCategoriaPorIdQuery(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [Authorize(Policy = Policies.GerenciaEstoque)]
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Criar(
        [FromBody] CriarCategoriaCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.ToCreatedResult(this, nameof(ObterPorId), dto => new { id = dto.Id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.GerenciaEstoque)]
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Atualizar(
        Guid id,
        [FromBody] AtualizarCategoriaCommand command,
        CancellationToken cancellationToken)
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
        var result = await _sender.Send(new ExcluirCategoriaCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }
}
