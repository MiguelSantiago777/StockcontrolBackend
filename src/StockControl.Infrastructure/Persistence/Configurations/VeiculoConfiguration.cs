using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockControl.Domain.Aggregates;
using StockControl.Domain.ValueObjects;

namespace StockControl.Infrastructure.Persistence.Configurations;

public sealed class VeiculoConfiguration : IEntityTypeConfiguration<Veiculo>
{
    public void Configure(EntityTypeBuilder<Veiculo> builder)
    {
        builder.ToTable("veiculos");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Placa)
            .HasConversion(placa => placa.Value, value => Placa.Create(value).Value)
            .HasColumnName("placa")
            .HasMaxLength(7)
            .IsRequired();

        builder.HasIndex(v => v.Placa).IsUnique().HasFilter("\"DeletedAt\" IS NULL");

        builder.Property(v => v.Tipo).HasConversion<int>();
        builder.Property(v => v.Modelo).HasMaxLength(100);

        builder.Property(v => v.Version).IsRowVersion();
        builder.HasQueryFilter(v => v.DeletedAt == null);
    }
}
