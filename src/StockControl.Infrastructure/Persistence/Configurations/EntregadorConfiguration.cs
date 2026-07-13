using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockControl.Domain.Aggregates;
using StockControl.Domain.ValueObjects;

namespace StockControl.Infrastructure.Persistence.Configurations;

public sealed class EntregadorConfiguration : IEntityTypeConfiguration<Entregador>
{
    public void Configure(EntityTypeBuilder<Entregador> builder)
    {
        builder.ToTable("entregadores");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome).HasMaxLength(200).IsRequired();

        builder.Property(e => e.Cpf)
            .HasConversion(cpf => cpf.Value, value => Cpf.Create(value).Value)
            .HasColumnName("cpf")
            .HasMaxLength(11)
            .IsRequired();

        builder.HasIndex(e => e.Cpf).IsUnique().HasFilter("\"DeletedAt\" IS NULL");

        builder.OwnsOne(e => e.Telefone, telefone =>
        {
            telefone.Property(t => t.Ddd).HasColumnName("telefone_ddd").HasMaxLength(2);
            telefone.Property(t => t.Numero).HasColumnName("telefone_numero").HasMaxLength(9);
        });

        builder.OwnsOne(e => e.UltimaPosicao, posicao =>
        {
            posicao.Property(p => p.Latitude).HasColumnName("ultima_latitude");
            posicao.Property(p => p.Longitude).HasColumnName("ultima_longitude");
        });

        builder.Property(e => e.Status).HasConversion<int>();
        builder.Property(e => e.Version).IsRowVersion();
        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}
