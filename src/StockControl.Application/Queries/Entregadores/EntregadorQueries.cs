using MediatR;
using StockControl.Application.Commands.Entregadores;
using StockControl.Application.Common;
using StockControl.Application.DTOs;
using StockControl.Application.Interfaces;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Common;
using StockControl.Domain.Enums;
using StockControl.Domain.Repositories;
using StockControl.Domain.Specifications;

namespace StockControl.Application.Queries.Entregadores;

// ─── Listar (paginado) ──────────────────────────────────────────────────────

public sealed class ListarEntregadoresQuery : IRequest<Result<PagedResultDto<EntregadorDto>>>
{
    public ListarEntregadoresQuery(int page = 1, int pageSize = 20, string? busca = null)
    {
        Page = page < 1 ? 1 : page;
        PageSize = pageSize is < 1 or > 100 ? 20 : pageSize;
        Busca = busca;
    }

    public int Page { get; }
    public int PageSize { get; }
    public string? Busca { get; }
}

public sealed class ListarEntregadoresQueryHandler : IRequestHandler<ListarEntregadoresQuery, Result<PagedResultDto<EntregadorDto>>>
{
    private readonly IRepository<Entregador> _repository;
    private readonly ICacheService _cache;

    public ListarEntregadoresQueryHandler(IRepository<Entregador> repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Result<PagedResultDto<EntregadorDto>>> Handle(
        ListarEntregadoresQuery request,
        CancellationToken cancellationToken)
    {
        var usaCache = request.Page == 1 && request.Busca is null;

        if (usaCache)
        {
            var cached = await _cache.GetAsync<PagedResultDto<EntregadorDto>>(CacheKeys.Entregadores, cancellationToken);
            if (cached is not null)
            {
                return Result.Success(cached);
            }
        }

        var spec = new EntregadoresAtivosSpecification(request.Page, request.PageSize, request.Busca);
        var entregadores = await _repository.ListAsync(spec, cancellationToken);
        var totalCount = await _repository.CountAsync(spec, cancellationToken);

        var resultado = new PagedResultDto<EntregadorDto>
        {
            Items = entregadores.Select(e => e.ToDto()).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        if (usaCache)
        {
            await _cache.SetAsync(CacheKeys.Entregadores, resultado, TimeSpan.FromMinutes(5), cancellationToken);
        }

        return Result.Success(resultado);
    }
}

// ─── Obter por Id ───────────────────────────────────────────────────────────

public sealed class ObterEntregadorPorIdQuery : IRequest<Result<EntregadorDto>>
{
    public ObterEntregadorPorIdQuery(Guid id) => Id = id;

    public Guid Id { get; }
}

public sealed class ObterEntregadorPorIdQueryHandler : IRequestHandler<ObterEntregadorPorIdQuery, Result<EntregadorDto>>
{
    private readonly IRepository<Entregador> _repository;

    public ObterEntregadorPorIdQueryHandler(IRepository<Entregador> repository)
    {
        _repository = repository;
    }

    public async Task<Result<EntregadorDto>> Handle(ObterEntregadorPorIdQuery request, CancellationToken cancellationToken)
    {
        var entregador = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entregador is null)
        {
            return Result.Failure<EntregadorDto>(Error.NotFound("Entregador.NaoEncontrado", "Entregador não encontrado."));
        }

        return entregador.ToDto();
    }
}

// ─── Usuários elegíveis (perfil Entregador, ainda não vinculados) ───────────

public sealed class UsuarioElegivelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public sealed class ListarUsuariosElegiveisParaEntregadorQuery : IRequest<Result<IReadOnlyList<UsuarioElegivelDto>>>
{
}

public sealed class ListarUsuariosElegiveisParaEntregadorQueryHandler
    : IRequestHandler<ListarUsuariosElegiveisParaEntregadorQuery, Result<IReadOnlyList<UsuarioElegivelDto>>>
{
    private readonly IRepository<Usuario> _usuarioRepository;
    private readonly IRepository<Entregador> _entregadorRepository;

    public ListarUsuariosElegiveisParaEntregadorQueryHandler(
        IRepository<Usuario> usuarioRepository,
        IRepository<Entregador> entregadorRepository)
    {
        _usuarioRepository = usuarioRepository;
        _entregadorRepository = entregadorRepository;
    }

    public async Task<Result<IReadOnlyList<UsuarioElegivelDto>>> Handle(
        ListarUsuariosElegiveisParaEntregadorQuery request,
        CancellationToken cancellationToken)
    {
        var usuarios = await _usuarioRepository.ListAsync(cancellationToken);
        var entregadores = await _entregadorRepository.ListAsync(cancellationToken);
        var usuariosVinculados = entregadores.Select(e => e.UsuarioId).ToHashSet();

        var elegiveis = usuarios
            .Where(u => u.Perfil == PerfilUsuario.Entregador && !usuariosVinculados.Contains(u.Id))
            .Select(u => new UsuarioElegivelDto { Id = u.Id, Name = u.Nome, Email = u.Email.Value })
            .ToList();

        return Result.Success<IReadOnlyList<UsuarioElegivelDto>>(elegiveis);
    }
}
