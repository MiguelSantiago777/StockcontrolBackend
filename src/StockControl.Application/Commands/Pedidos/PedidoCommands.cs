using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using StockControl.Application.Common;
using StockControl.Application.DTOs;
using StockControl.Application.Interfaces;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Common;
using StockControl.Domain.Enums;
using StockControl.Domain.Interfaces;
using StockControl.Domain.Repositories;
using StockControl.Domain.Services;

namespace StockControl.Application.Commands.Pedidos;

// ─── Criar ───────────────────────────────────────────────────────────────────

public sealed class ItemPedidoInput
{
    [JsonPropertyName("productId")]
    public Guid ProductId { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}

public sealed class CriarPedidoCommand : IRequest<Result<PedidoDto>>
{
    [JsonPropertyName("customerId")]
    public Guid CustomerId { get; set; }

    [JsonPropertyName("items")]
    public List<ItemPedidoInput> Items { get; set; } = [];
}

public sealed class CriarPedidoCommandValidator : AbstractValidator<CriarPedidoCommand>
{
    public CriarPedidoCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("O cliente é obrigatório.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("Adicione ao menos um item ao pedido.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty().WithMessage("Produto inválido.");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");
        });
    }
}

public sealed class CriarPedidoCommandHandler : IRequestHandler<CriarPedidoCommand, Result<PedidoDto>>
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IMovimentacaoRepository _movimentacaoRepository;
    private readonly EstoqueDomainService _estoqueDomainService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ICurrentUserService _currentUser;

    public CriarPedidoCommandHandler(
        IPedidoRepository pedidoRepository,
        IRepository<Cliente> clienteRepository,
        IProdutoRepository produtoRepository,
        IMovimentacaoRepository movimentacaoRepository,
        EstoqueDomainService estoqueDomainService,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ICurrentUserService currentUser)
    {
        _pedidoRepository = pedidoRepository;
        _clienteRepository = clienteRepository;
        _produtoRepository = produtoRepository;
        _movimentacaoRepository = movimentacaoRepository;
        _estoqueDomainService = estoqueDomainService;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _currentUser = currentUser;
    }

    public async Task<Result<PedidoDto>> Handle(CriarPedidoCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } usuarioId)
        {
            return Result.Failure<PedidoDto>(Error.Unauthorized("Auth.NaoAutenticado", "Usuário não autenticado."));
        }

        var cliente = await _clienteRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (cliente is null)
        {
            return Result.Failure<PedidoDto>(Error.NotFound("Cliente.NaoEncontrado", "Cliente não encontrado."));
        }

        var pedidoResult = Pedido.Criar(cliente.Id, cliente.Endereco);
        if (pedidoResult.IsFailure)
        {
            return Result.Failure<PedidoDto>(pedidoResult.Error);
        }

        var pedido = pedidoResult.Value;
        var produtosAtualizados = new List<Produto>();
        var movimentacoes = new List<Movimentacao>();

        foreach (var item in request.Items)
        {
            var produto = await _produtoRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (produto is null)
            {
                return Result.Failure<PedidoDto>(Error.NotFound("Produto.NaoEncontrado", "Produto não encontrado."));
            }

            var adicionarResult = pedido.AdicionarItem(produto.Id, produto.Nome, produto.Preco.Amount, item.Quantity);
            if (adicionarResult.IsFailure)
            {
                return Result.Failure<PedidoDto>(adicionarResult.Error);
            }

            var movimentacaoResult = _estoqueDomainService.Movimentar(
                produto, TipoMovimentacao.Saida, item.Quantity, usuarioId, $"Pedido {pedido.Id}");
            if (movimentacaoResult.IsFailure)
            {
                return Result.Failure<PedidoDto>(movimentacaoResult.Error);
            }

            produtosAtualizados.Add(produto);
            movimentacoes.Add(movimentacaoResult.Value);
        }

        await _pedidoRepository.AddAsync(pedido, cancellationToken);
        foreach (var produto in produtosAtualizados)
        {
            _produtoRepository.Update(produto);
        }

        foreach (var movimentacao in movimentacoes)
        {
            await _movimentacaoRepository.AddAsync(movimentacao, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Pedidos, cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Produtos, cancellationToken);

        return pedido.ToDto(cliente.Nome, null, null);
    }
}

// ─── Iniciar entrega ─────────────────────────────────────────────────────────

public sealed class IniciarEntregaPedidoCommand : IRequest<Result<PedidoDto>>
{
    [JsonIgnore]
    public Guid Id { get; set; }

    [JsonPropertyName("driverId")]
    public Guid DriverId { get; set; }
}

public sealed class IniciarEntregaPedidoCommandValidator : AbstractValidator<IniciarEntregaPedidoCommand>
{
    public IniciarEntregaPedidoCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.DriverId).NotEmpty().WithMessage("O entregador é obrigatório.");
    }
}

public sealed class IniciarEntregaPedidoCommandHandler : IRequestHandler<IniciarEntregaPedidoCommand, Result<PedidoDto>>
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IEntregadorRepository _entregadorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public IniciarEntregaPedidoCommandHandler(
        IPedidoRepository pedidoRepository,
        IRepository<Cliente> clienteRepository,
        IEntregadorRepository entregadorRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache)
    {
        _pedidoRepository = pedidoRepository;
        _clienteRepository = clienteRepository;
        _entregadorRepository = entregadorRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<PedidoDto>> Handle(IniciarEntregaPedidoCommand request, CancellationToken cancellationToken)
    {
        var pedido = await _pedidoRepository.ObterComItensAsync(request.Id, cancellationToken);
        if (pedido is null)
        {
            return Result.Failure<PedidoDto>(Error.NotFound("Pedido.NaoEncontrado", "Pedido não encontrado."));
        }

        var entregador = await _entregadorRepository.GetByIdAsync(request.DriverId, cancellationToken);
        if (entregador is null)
        {
            return Result.Failure<PedidoDto>(Error.NotFound("Entregador.NaoEncontrado", "Entregador não encontrado."));
        }

        if (entregador.Status == StatusEntregador.Indisponivel)
        {
            return Result.Failure<PedidoDto>(
                Error.Conflict("Entregador.Indisponivel", "O entregador selecionado está indisponível."));
        }

        var result = pedido.IniciarEntrega(entregador.Id);
        if (result.IsFailure)
        {
            return Result.Failure<PedidoDto>(result.Error);
        }

        entregador.IniciarEntrega();

        _pedidoRepository.Update(pedido);
        _entregadorRepository.Update(entregador);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Pedidos, cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Entregadores, cancellationToken);

        var cliente = await _clienteRepository.GetByIdAsync(pedido.ClienteId, cancellationToken);
        return pedido.ToDto(cliente?.Nome ?? "—", entregador.Id, entregador.Nome);
    }
}

// ─── Finalizar entrega ───────────────────────────────────────────────────────

public sealed class FinalizarEntregaPedidoCommand : IRequest<Result<PedidoDto>>
{
    public FinalizarEntregaPedidoCommand(Guid id) => Id = id;

    public Guid Id { get; }
}

public sealed class FinalizarEntregaPedidoCommandHandler : IRequestHandler<FinalizarEntregaPedidoCommand, Result<PedidoDto>>
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IEntregadorRepository _entregadorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public FinalizarEntregaPedidoCommandHandler(
        IPedidoRepository pedidoRepository,
        IRepository<Cliente> clienteRepository,
        IEntregadorRepository entregadorRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache)
    {
        _pedidoRepository = pedidoRepository;
        _clienteRepository = clienteRepository;
        _entregadorRepository = entregadorRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<PedidoDto>> Handle(FinalizarEntregaPedidoCommand request, CancellationToken cancellationToken)
    {
        var pedido = await _pedidoRepository.ObterComItensAsync(request.Id, cancellationToken);
        if (pedido is null)
        {
            return Result.Failure<PedidoDto>(Error.NotFound("Pedido.NaoEncontrado", "Pedido não encontrado."));
        }

        var result = pedido.FinalizarEntrega();
        if (result.IsFailure)
        {
            return Result.Failure<PedidoDto>(result.Error);
        }

        Entregador? entregador = null;
        if (pedido.EntregadorId is { } entregadorId)
        {
            entregador = await _entregadorRepository.GetByIdAsync(entregadorId, cancellationToken);
            entregador?.FicarDisponivel();
            if (entregador is not null) _entregadorRepository.Update(entregador);
        }

        _pedidoRepository.Update(pedido);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Pedidos, cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Entregadores, cancellationToken);

        var cliente = await _clienteRepository.GetByIdAsync(pedido.ClienteId, cancellationToken);
        return pedido.ToDto(cliente?.Nome ?? "—", entregador?.Id, entregador?.Nome);
    }
}

// ─── Cancelar ────────────────────────────────────────────────────────────────

public sealed class CancelarPedidoCommand : IRequest<Result<PedidoDto>>
{
    public CancelarPedidoCommand(Guid id) => Id = id;

    public Guid Id { get; }
}

public sealed class CancelarPedidoCommandHandler : IRequestHandler<CancelarPedidoCommand, Result<PedidoDto>>
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IMovimentacaoRepository _movimentacaoRepository;
    private readonly EstoqueDomainService _estoqueDomainService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ICurrentUserService _currentUser;

    public CancelarPedidoCommandHandler(
        IPedidoRepository pedidoRepository,
        IRepository<Cliente> clienteRepository,
        IProdutoRepository produtoRepository,
        IMovimentacaoRepository movimentacaoRepository,
        EstoqueDomainService estoqueDomainService,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ICurrentUserService currentUser)
    {
        _pedidoRepository = pedidoRepository;
        _clienteRepository = clienteRepository;
        _produtoRepository = produtoRepository;
        _movimentacaoRepository = movimentacaoRepository;
        _estoqueDomainService = estoqueDomainService;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _currentUser = currentUser;
    }

    public async Task<Result<PedidoDto>> Handle(CancelarPedidoCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } usuarioId)
        {
            return Result.Failure<PedidoDto>(Error.Unauthorized("Auth.NaoAutenticado", "Usuário não autenticado."));
        }

        var pedido = await _pedidoRepository.ObterComItensAsync(request.Id, cancellationToken);
        if (pedido is null)
        {
            return Result.Failure<PedidoDto>(Error.NotFound("Pedido.NaoEncontrado", "Pedido não encontrado."));
        }

        var result = pedido.Cancelar();
        if (result.IsFailure)
        {
            return Result.Failure<PedidoDto>(result.Error);
        }

        foreach (var item in pedido.Itens)
        {
            var produto = await _produtoRepository.GetByIdAsync(item.ProdutoId, cancellationToken);
            if (produto is null) continue;

            var movimentacaoResult = _estoqueDomainService.Movimentar(
                produto, TipoMovimentacao.Devolucao, item.Quantidade.Value, usuarioId, $"Cancelamento do pedido {pedido.Id}");
            if (movimentacaoResult.IsFailure) continue;

            _produtoRepository.Update(produto);
            await _movimentacaoRepository.AddAsync(movimentacaoResult.Value, cancellationToken);
        }

        _pedidoRepository.Update(pedido);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Pedidos, cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Produtos, cancellationToken);

        var cliente = await _clienteRepository.GetByIdAsync(pedido.ClienteId, cancellationToken);
        return pedido.ToDto(cliente?.Nome ?? "—", null, null);
    }
}

// ─── Mapeamento simples (sem AutoMapper: entidade pequena) ───────────────────

internal static class PedidoMapping
{
    public static PedidoDto ToDto(this Pedido pedido, string customerName, Guid? driverId, string? driverName) =>
        new()
        {
            Id = pedido.Id,
            CustomerId = pedido.ClienteId,
            CustomerName = customerName,
            DriverId = driverId,
            DriverName = driverName,
            Status = pedido.Status.ToString(),
            Total = pedido.Total.Amount,
            Items = pedido.Itens.Select(i => new ItemPedidoDto
            {
                ProductId = i.ProdutoId,
                ProductName = i.NomeProduto,
                UnitPrice = i.PrecoUnitario.Amount,
                Quantity = i.Quantidade.Value,
                Subtotal = i.PrecoUnitario.Amount * i.Quantidade.Value
            }).ToList(),
            CreatedAt = pedido.CreatedAt
        };
}
