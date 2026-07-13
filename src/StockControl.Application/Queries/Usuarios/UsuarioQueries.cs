using MediatR;
using StockControl.Application.Commands.Usuarios;
using StockControl.Application.Common;
using StockControl.Application.DTOs;
using StockControl.Application.Interfaces;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Common;
using StockControl.Domain.Repositories;
using StockControl.Domain.Specifications;

namespace StockControl.Application.Queries.Usuarios;

// ─── Listar (paginado) ──────────────────────────────────────────────────────

public sealed class ListarUsuariosQuery : IRequest<Result<PagedResultDto<UsuarioDto>>>
{
    public ListarUsuariosQuery(int page = 1, int pageSize = 20, string? busca = null)
    {
        Page = page < 1 ? 1 : page;
        PageSize = pageSize is < 1 or > 100 ? 20 : pageSize;
        Busca = busca;
    }

    public int Page { get; }
    public int PageSize { get; }
    public string? Busca { get; }
}

public sealed class ListarUsuariosQueryHandler : IRequestHandler<ListarUsuariosQuery, Result<PagedResultDto<UsuarioDto>>>
{
    private readonly IRepository<Usuario> _repository;
    private readonly ICacheService _cache;

    public ListarUsuariosQueryHandler(IRepository<Usuario> repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Result<PagedResultDto<UsuarioDto>>> Handle(
        ListarUsuariosQuery request,
        CancellationToken cancellationToken)
    {
        var usaCache = request.Page == 1 && request.Busca is null;

        if (usaCache)
        {
            var cached = await _cache.GetAsync<PagedResultDto<UsuarioDto>>(CacheKeys.Usuarios, cancellationToken);
            if (cached is not null)
            {
                return Result.Success(cached);
            }
        }

        var spec = new UsuariosAtivosSpecification(request.Page, request.PageSize, request.Busca);
        var usuarios = await _repository.ListAsync(spec, cancellationToken);
        var totalCount = await _repository.CountAsync(spec, cancellationToken);

        var resultado = new PagedResultDto<UsuarioDto>
        {
            Items = usuarios.Select(u => u.ToDto()).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        if (usaCache)
        {
            await _cache.SetAsync(CacheKeys.Usuarios, resultado, TimeSpan.FromMinutes(5), cancellationToken);
        }

        return Result.Success(resultado);
    }
}

// ─── Obter por Id ───────────────────────────────────────────────────────────

public sealed class ObterUsuarioPorIdQuery : IRequest<Result<UsuarioDto>>
{
    public ObterUsuarioPorIdQuery(Guid id) => Id = id;

    public Guid Id { get; }
}

public sealed class ObterUsuarioPorIdQueryHandler : IRequestHandler<ObterUsuarioPorIdQuery, Result<UsuarioDto>>
{
    private readonly IRepository<Usuario> _repository;

    public ObterUsuarioPorIdQueryHandler(IRepository<Usuario> repository)
    {
        _repository = repository;
    }

    public async Task<Result<UsuarioDto>> Handle(ObterUsuarioPorIdQuery request, CancellationToken cancellationToken)
    {
        var usuario = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (usuario is null)
        {
            return Result.Failure<UsuarioDto>(
                Error.NotFound("Usuario.NaoEncontrado", "Usuário não encontrado."));
        }

        return usuario.ToDto();
    }
}
