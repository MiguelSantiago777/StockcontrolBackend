using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockControl.Domain.Entities;

namespace StockControl.Infrastructure.Persistence.Configurations;

public sealed class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("categorias");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome).HasMaxLength(100).IsRequired();
        builder.HasIndex(c => c.Nome).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
        builder.Property(c => c.Descricao).HasMaxLength(500);

        builder.Property(c => c.Version).IsRowVersion();
        builder.HasQueryFilter(c => c.DeletedAt == null);
    }
}
