using StockControl.Domain.Common;

namespace StockControl.Domain.ValueObjects;

public sealed class Cpf : ValueObject
{
    private Cpf(string value) => Value = value;
    public string Value { get; }

    public static Result<Cpf> Create(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return Result.Failure<Cpf>(Error.Validation("Cpf.Vazio", "O CPF é obrigatório."));

        var digits = new string(cpf.Where(char.IsDigit).ToArray());

        if (digits.Length != 11 || digits.Distinct().Count() == 1 || !ValidarDigitos(digits))
            return Result.Failure<Cpf>(Error.Validation("Cpf.Invalido", "CPF inválido."));

        return new Cpf(digits);
    }

    private static bool ValidarDigitos(string cpf)
    {
        for (var j = 9; j < 11; j++)
        {
            var soma = 0;
            for (var i = 0; i < j; i++)
                soma += (cpf[i] - '0') * (j + 1 - i);
            var digito = soma % 11 < 2 ? 0 : 11 - soma % 11;
            if (cpf[j] - '0' != digito) return false;
        }
        return true;
    }

    public string Formatado => $"{Value[..3]}.{Value[3..6]}.{Value[6..9]}-{Value[9..]}";
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Formatado;
}
