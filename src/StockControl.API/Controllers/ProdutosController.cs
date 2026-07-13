using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockControl.API.Extensions;
using StockControl.Application.Commands.Produtos;
using StockControl.Application.DTOs;
using StockControl.Application.Queries.Produtos;
using static StockControl.Infrastructure.DependencyInjection;

namespace StockControl.API.Controllers;

[ApiController]
[Route("api/v1/products")]
[Authorize]
public sealed class ProdutosController : ControllerBase
{
    private const long TamanhoMaximoImagemBytes = 5 * 1024 * 1024; // 5 MB

    private readonly ISender _sender;

    public ProdutosController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Leitura)]
    [ProducesResponseType(typeof(PagedResultDto<ProdutoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListarProdutosQuery(page, pageSize, search), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.Leitura)]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ObterProdutoPorIdQuery(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [Authorize(Policy = Policies.GerenciaEstoque)]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Criar(
        [FromBody] CriarProdutoCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.ToCreatedResult(this, nameof(ObterPorId), dto => new { id = dto.Id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.GerenciaEstoque)]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Atualizar(
        Guid id,
        [FromBody] AtualizarProdutoCommand command,
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
        var result = await _sender.Send(new ExcluirProdutoCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/image")]
    [Authorize(Policy = Policies.GerenciaEstoque)]
    [ProducesResponseType(typeof(UploadImagemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [RequestSizeLimit(TamanhoMaximoImagemBytes)]
    public async Task<ActionResult> UploadImagem(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return Problem(title: "Arquivo inválido", detail: "Nenhum arquivo enviado.", statusCode: StatusCodes.Status400BadRequest);
        }

        await using var stream = file.OpenReadStream();

        var command = new UploadImagemProdutoCommand
        {
            ProdutoId = id,
            Conteudo = stream,
            NomeArquivo = file.FileName,
            ContentType = file.ContentType
        };

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }

        return Ok(new UploadImagemResponse(result.Value.ImagemUrl ?? string.Empty));
    }
}

public sealed record UploadImagemResponse(string Url);
