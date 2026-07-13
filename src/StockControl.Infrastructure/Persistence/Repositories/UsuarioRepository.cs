using Microsoft.EntityFrameworkCore;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Repositories;
using StockControl.Domain.ValueObjects;
using StockControl.Infrastructure.Persistence.Context;

namespace StockControl.Infrastructure.Persistence.Repositories;

public sealed class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<Usuario?> ObterPorEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(usuario => usuario.Email == email, cancellationToken);
    }

    public async Task<bool> EmailExisteAsync(Email email, Guid? ignorarId = null, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(usuario => usuario.Email == email && usuario.Id != ignorarId, cancellationToken);
    }

    public async Task<Usuario?> ObterPorRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(usuario => usuario.RefreshToken == refreshToken, cancellationToken);
    }
}
