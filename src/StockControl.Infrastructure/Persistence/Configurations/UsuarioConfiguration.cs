using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockControl.Domain.Aggregates;
using StockControl.Domain.ValueObjects;

namespace StockControl.Infrastructure.Persistence.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nome).HasMaxLength(200).IsRequired();

        builder.Property(u => u.Email)
            .HasConversion(e => e.Value, v => Email.Create(v).Value)
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(u => u.Email).IsUnique().HasFilter("\"DeletedAt\" IS NULL");

        builder.Property(u => u.Senha)
            .HasConversion(s => s.Value, v => SenhaHash.Create(v).Value)
            .HasColumnName("senha_hash")
            .IsRequired();

        builder.Property(u => u.Perfil).HasConversion<int>();
        builder.Property(u => u.RefreshToken).HasMaxLength(500);
        builder.Property(u => u.Version).IsRowVersion();

        builder.HasQueryFilter(u => u.DeletedAt == null);
    }
}
