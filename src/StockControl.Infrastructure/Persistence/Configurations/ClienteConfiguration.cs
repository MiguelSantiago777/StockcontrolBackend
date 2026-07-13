using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockControl.Domain.Aggregates;
using StockControl.Domain.ValueObjects;

namespace StockControl.Infrastructure.Persistence.Configurations;

public sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome).HasMaxLength(200).IsRequired();

        builder.Property(c => c.Cpf)
            .HasConversion(cpf => cpf.Value, value => Cpf.Create(value).Value)
            .HasColumnName("cpf")
            .HasMaxLength(11)
            .IsRequired();

        builder.HasIndex(c => c.Cpf).IsUnique().HasFilter("\"DeletedAt\" IS NULL");

        builder.Property(c => c.Email)
            .HasConversion(email => email.Value, value => Email.Create(value).Value)
            .HasColumnName("email")
            .HasMaxLength(320)
            .IsRequired();

        builder.OwnsOne(c => c.Telefone, telefone =>
        {
            telefone.Property(t => t.Ddd).HasColumnName("telefone_ddd").HasMaxLength(2);
            telefone.Property(t => t.Numero).HasColumnName("telefone_numero").HasMaxLength(9);
        });

        builder.OwnsOne(c => c.Endereco, endereco =>
        {
            endereco.Property(e => e.Logradouro).HasColumnName("logradouro").HasMaxLength(200);
            endereco.Property(e => e.Numero).HasColumnName("numero").HasMaxLength(20);
            endereco.Property(e => e.Complemento).HasColumnName("complemento").HasMaxLength(100);
            endereco.Property(e => e.Bairro).HasColumnName("bairro").HasMaxLength(100);
            endereco.Property(e => e.Cidade).HasColumnName("cidade").HasMaxLength(100);
            endereco.Property(e => e.Uf).HasColumnName("uf").HasMaxLength(2);
            endereco.Property(e => e.Cep).HasColumnName("cep").HasMaxLength(8);
        });

        builder.Property(c => c.Version).IsRowVersion();
        builder.HasQueryFilter(c => c.DeletedAt == null);
    }
}
