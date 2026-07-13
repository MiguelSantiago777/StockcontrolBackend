using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockControl.Domain.Aggregates;
using StockControl.Domain.ValueObjects;

namespace StockControl.Infrastructure.Persistence.Configurations;

public sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("produtos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Descricao).HasMaxLength(1000);

        // Value Objects como conversões / owned types
        builder.Property(p => p.Codigo)
            .HasConversion(c => c.Value, v => CodigoProduto.Create(v).Value)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(p => p.Codigo).IsUnique();

        builder.Property(p => p.CodigoBarras)
            .HasConversion(c => c!.Value, v => CodigoBarras.Create(v).Value)
            .HasMaxLength(14);

        builder.OwnsOne(p => p.Preco, preco =>
        {
            preco.Property(m => m.Amount).HasColumnName("preco").HasPrecision(18, 2);
            preco.Property(m => m.Currency).HasColumnName("moeda").HasMaxLength(3);
        });

        builder.Property(p => p.Estoque)
            .HasConversion(q => q.Value, v => Quantidade.Create(v).Value)
            .HasColumnName("estoque");

        builder.Property(p => p.EstoqueMinimo)
            .HasConversion(q => q.Value, v => Quantidade.Create(v).Value)
            .HasColumnName("estoque_minimo");

        builder.Property(p => p.ImagemUrl).HasColumnName("imagem_url").HasMaxLength(500);

        // Concorrência otimista com xmin do PostgreSQL
        builder.Property(p => p.Version).IsRowVersion();

        builder.HasQueryFilter(p => p.DeletedAt == null);
    }
}
