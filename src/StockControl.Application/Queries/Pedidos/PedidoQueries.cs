using MediatR;
using StockControl.Application.Commands.Pedidos;
using StockControl.Application.DTOs;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Common;
using StockControl.Domain.Enums;
using StockControl.Domain.Repositories;
using StockControl.Domain.Specifications;

namespace StockControl.Application.Queries.Pedidos;

// ─── Listar (paginado) ──────────────────────────────────────────────────────

public sealed class ListarPedidosQuery : IRequest<Result<PagedResultDto<PedidoDto>>>
{
    public ListarPedidosQuery(int page = 1, int pageSize = 20, string? status = null)
    {
        Page = page < 1 ? 1 : page;
        PageSize = pageSize is < 1 or > 100 ? 20 : pageSize;
        Status = Enum.TryParse<StatusPedido>(status, out var parsed) ? parsed : null;
    }

    public int Page { get; }
    public int PageSize { get; }
    public StatusPedido? Status { get; }
}

public sealed class ListarPedidosQueryHandler : IRequestHandler<ListarPedidosQuery, Result<PagedResultDto<PedidoDto>>>
{
    private readonly IRepository<Pedido> _pedidoRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<Entregador> _entregadorRepository;

    public ListarPedidosQueryHandler(
        IRepository<Pedido> pedidoRepository,
        IRepository<Cliente> clienteRepository,
        IRepository<Entregador> entregadorRepository)
    {
        _pedidoRepository = pedidoRepository;
        _clienteRepository = clienteRepository;
        _entregadorRepository = entregadorRepository;
    }

    public async Task<Result<PagedResultDto<PedidoDto>>> Handle(
        ListarPedidosQuery request,
        CancellationToken cancellationToken)
    {
        var spec = new PedidosSpecification(request.Page, request.PageSize, request.Status);
        var pedidos = await _pedidoRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _pedidoRepository.CountAsync(spec, cancellationToken);

        var clientes = (await _clienteRepository.ListAsync(cancellationToken)).ToDictionary(c => c.Id, c => c.Nome);
        var entregadores = (await _entregadorRepository.ListAsync(cancellationToken)).ToDictionary(e => e.Id, e => e.Nome);

        var dtos = pedidos.Select(p => p.ToDto(
            clientes.GetValueOrDefault(p.ClienteId, "—"),
            p.EntregadorId,
            p.EntregadorId is { } id ? entregadores.GetValueOrDefault(id) : null
        )).ToList();

        var resultado = new PagedResultDto<PedidoDto>
        {
            Items = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return Result.Success(resultado);
    }
}

// ─── Obter por Id ───────────────────────────────────────────────────────────

public sealed class ObterPedidoPorIdQuery : IRequest<Result<PedidoDto>>
{
    public ObterPedidoPorIdQuery(Guid id) => Id = id;

    public Guid Id { get; }
}

public sealed class ObterPedidoPorIdQueryHandler : IRequestHandler<ObterPedidoPorIdQuery, Result<PedidoDto>>
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<Entregador> _entregadorRepository;

    public ObterPedidoPorIdQueryHandler(
        IPedidoRepository pedidoRepository,
        IRepository<Cliente> clienteRepository,
        IRepository<Entregador> entregadorRepository)
    {
        _pedidoRepository = pedidoRepository;
        _clienteRepository = clienteRepository;
        _entregadorRepository = entregadorRepository;
    }

    public async Task<Result<PedidoDto>> Handle(ObterPedidoPorIdQuery request, CancellationToken cancellationToken)
    {
        var pedido = await _pedidoRepository.ObterComItensAsync(request.Id, cancellationToken);
        if (pedido is null)
        {
            return Result.Failure<PedidoDto>(Error.NotFound("Pedido.NaoEncontrado", "Pedido não encontrado."));
        }

        var cliente = await _clienteRepository.GetByIdAsync(pedido.ClienteId, cancellationToken);
        var entregador = pedido.EntregadorId is { } id
            ? await _entregadorRepository.GetByIdAsync(id, cancellationToken)
            : null;

        return pedido.ToDto(cliente?.Nome ?? "—", entregador?.Id, entregador?.Nome);
    }
}
