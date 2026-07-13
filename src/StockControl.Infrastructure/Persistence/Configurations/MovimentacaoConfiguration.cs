using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockControl.Domain.Aggregates;
using StockControl.Domain.ValueObjects;

namespace StockControl.Infrastructure.Persistence.Configurations;

public sealed class MovimentacaoConfiguration : IEntityTypeConfiguration<Movimentacao>
{
    public void Configure(EntityTypeBuilder<Movimentacao> builder)
    {
        builder.ToTable("movimentacoes");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Tipo).HasConversion<int>();

        builder.Property(m => m.Quantidade)
            .HasConversion(quantidade => quantidade.Value, value => Quantidade.Create(value).Value)
            .HasColumnName("quantidade");

        builder.Property(m => m.Observacao).HasMaxLength(500);

        builder.HasIndex(m => m.ProdutoId);
        builder.Property(m => m.Version).IsRowVersion();
        builder.HasQueryFilter(m => m.DeletedAt == null);
    }
}
