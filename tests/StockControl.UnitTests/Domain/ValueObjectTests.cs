using FluentAssertions;
using StockControl.Domain.ValueObjects;

namespace StockControl.UnitTests.Domain;

public class ValueObjectTests
{
    [Theory]
    [InlineData("usuario@empresa.com.br", true)]
    [InlineData("invalido@", false)]
    [InlineData("", false)]
    public void Email_Create_DeveValidarFormato(string email, bool esperado)
    {
        Email.Create(email).IsSuccess.Should().Be(esperado);
    }

    [Theory]
    [InlineData("529.982.247-25", true)]   // CPF válido conhecido
    [InlineData("111.111.111-11", false)]  // dígitos repetidos
    [InlineData("123.456.789-00", false)]
    public void Cpf_Create_DeveValidarDigitosVerificadores(string cpf, bool esperado)
    {
        Cpf.Create(cpf).IsSuccess.Should().Be(esperado);
    }

    [Fact]
    public void Money_Add_DeveSomarValores()
    {
        var a = Money.Create(10.50m).Value;
        var b = Money.Create(4.50m).Value;

        a.Add(b).Amount.Should().Be(15.00m);
    }

    [Fact]
    public void Money_Negativo_DeveFalhar()
    {
        Money.Create(-1).IsFailure.Should().BeTrue();
    }
}
