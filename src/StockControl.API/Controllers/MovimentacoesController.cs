using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockControl.API.Extensions;
using StockControl.Application.Commands.Movimentacoes;
using StockControl.Application.DTOs;
using StockControl.Application.Queries.Movimentacoes;
using static StockControl.Infrastructure.DependencyInjection;

namespace StockControl.API.Controllers;

[ApiController]
[Route("api/v1/movements")]
[Authorize]
public sealed class MovimentacoesController : ControllerBase
{
    private readonly ISender _sender;

    public MovimentacoesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Leitura)]
    [ProducesResponseType(typeof(PagedResultDto<MovimentacaoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? type = null,
        [FromQuery] Guid? productId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListarMovimentacoesQuery(page, pageSize, type, productId, from, to), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [Authorize(Policy = Policies.GerenciaEstoque)]
    [ProducesResponseType(typeof(MovimentacaoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Criar([FromBody] CriarMovimentacaoCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("export")]
    [Authorize(Policy = Policies.Leitura)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Exportar(
        [FromQuery] string? type = null,
        [FromQuery] Guid? productId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListarMovimentacoesQuery(1, 5000, type, productId, from, to), cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }

        var csv = new StringBuilder();
        csv.AppendLine("Data,Produto,Tipo,Quantidade,Usuario,Observacao");
        foreach (var m in result.Value.Items)
        {
            csv.AppendLine(string.Join(',',
                m.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                CsvEscape(m.ProductName),
                m.Type,
                m.Quantity,
                CsvEscape(m.UserName),
                CsvEscape(m.Note ?? string.Empty)));
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        return File(bytes, "text/csv", $"movimentacoes_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    private static string CsvEscape(string value) =>
        value.Contains(',') || value.Contains('"')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
}
