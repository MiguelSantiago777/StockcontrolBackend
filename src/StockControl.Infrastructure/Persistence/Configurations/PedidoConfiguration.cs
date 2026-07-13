using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Entities;
using StockControl.Domain.ValueObjects;

namespace StockControl.Infrastructure.Persistence.Configurations;

public sealed class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("pedidos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Status).HasConversion<int>();

        builder.OwnsOne(p => p.EnderecoEntrega, endereco =>
        {
            endereco.Property(e => e.Logradouro).HasColumnName("logradouro").HasMaxLength(200);
            endereco.Property(e => e.Numero).HasColumnName("numero").HasMaxLength(20);
            endereco.Property(e => e.Complemento).HasColumnName("complemento").HasMaxLength(100);
            endereco.Property(e => e.Bairro).HasColumnName("bairro").HasMaxLength(100);
            endereco.Property(e => e.Cidade).HasColumnName("cidade").HasMaxLength(100);
            endereco.Property(e => e.Uf).HasColumnName("uf").HasMaxLength(2);
            endereco.Property(e => e.Cep).HasColumnName("cep").HasMaxLength(8);
        });

        builder.HasMany(p => p.Itens)
            .WithOne()
            .HasForeignKey(i => i.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Itens).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(p => p.Version).IsRowVersion();
        builder.HasQueryFilter(p => p.DeletedAt == null);
    }
}

public sealed class ItemPedidoConfiguration : IEntityTypeConfiguration<ItemPedido>
{
    public void Configure(EntityTypeBuilder<ItemPedido> builder)
    {
        builder.ToTable("itens_pedido");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.NomeProduto).HasMaxLength(200).IsRequired();

        builder.OwnsOne(i => i.PrecoUnitario, preco =>
        {
            preco.Property(m => m.Amount).HasColumnName("preco_unitario").HasPrecision(18, 2);
            preco.Property(m => m.Currency).HasColumnName("moeda").HasMaxLength(3);
        });

        builder.Property(i => i.Quantidade)
            .HasConversion(q => q.Value, v => Quantidade.Create(v).Value)
            .HasColumnName("quantidade");
    }
}
