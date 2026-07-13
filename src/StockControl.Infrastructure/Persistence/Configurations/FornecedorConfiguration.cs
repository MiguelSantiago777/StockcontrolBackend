using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockControl.Domain.Aggregates;
using StockControl.Domain.ValueObjects;

namespace StockControl.Infrastructure.Persistence.Configurations;

public sealed class FornecedorConfiguration : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.ToTable("fornecedores");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.RazaoSocial).HasMaxLength(200).IsRequired();
        builder.Property(f => f.NomeFantasia).HasMaxLength(200);

        builder.Property(f => f.Cnpj)
            .HasConversion(cnpj => cnpj.Value, value => Cnpj.Create(value).Value)
            .HasColumnName("cnpj")
            .HasMaxLength(14)
            .IsRequired();

        builder.HasIndex(f => f.Cnpj).IsUnique();

        builder.Property(f => f.Email)
            .HasConversion(email => email.Value, value => Email.Create(value).Value)
            .HasColumnName("email")
            .HasMaxLength(320)
            .IsRequired();

        builder.OwnsOne(f => f.Telefone, telefone =>
        {
            telefone.Property(t => t.Ddd).HasColumnName("telefone_ddd").HasMaxLength(2);
            telefone.Property(t => t.Numero).HasColumnName("telefone_numero").HasMaxLength(9);
        });

        builder.OwnsOne(f => f.Endereco, endereco =>
        {
            endereco.Property(e => e.Logradouro).HasColumnName("logradouro").HasMaxLength(200);
            endereco.Property(e => e.Numero).HasColumnName("numero").HasMaxLength(20);
            endereco.Property(e => e.Complemento).HasColumnName("complemento").HasMaxLength(100);
            endereco.Property(e => e.Bairro).HasColumnName("bairro").HasMaxLength(100);
            endereco.Property(e => e.Cidade).HasColumnName("cidade").HasMaxLength(100);
            endereco.Property(e => e.Uf).HasColumnName("uf").HasMaxLength(2);
            endereco.Property(e => e.Cep).HasColumnName("cep").HasMaxLength(8);
        });

        builder.Property(f => f.Version).IsRowVersion();
        builder.HasQueryFilter(f => f.DeletedAt == null);
    }
}
