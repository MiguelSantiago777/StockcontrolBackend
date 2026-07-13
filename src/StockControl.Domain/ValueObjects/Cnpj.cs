using StockControl.Domain.Common;

namespace StockControl.Domain.ValueObjects;

public sealed class Cnpj : ValueObject
{
    private static readonly int[] Multiplicador1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] Multiplicador2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    private Cnpj(string value) => Value = value;
    public string Value { get; }

    public static Result<Cnpj> Create(string? cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return Result.Failure<Cnpj>(Error.Validation("Cnpj.Vazio", "O CNPJ é obrigatório."));

        var digits = new string(cnpj.Where(char.IsDigit).ToArray());

        if (digits.Length != 14 || digits.Distinct().Count() == 1 || !ValidarDigitos(digits))
            return Result.Failure<Cnpj>(Error.Validation("Cnpj.Invalido", "CNPJ inválido."));

        return new Cnpj(digits);
    }

    private static bool ValidarDigitos(string cnpj)
    {
        var soma = 0;
        for (var i = 0; i < 12; i++) soma += (cnpj[i] - '0') * Multiplicador1[i];
        var resto = soma % 11;
        var d1 = resto < 2 ? 0 : 11 - resto;
        if (cnpj[12] - '0' != d1) return false;

        soma = 0;
        for (var i = 0; i < 13; i++) soma += (cnpj[i] - '0') * Multiplicador2[i];
        resto = soma % 11;
        var d2 = resto < 2 ? 0 : 11 - resto;
        return cnpj[13] - '0' == d2;
    }

    public string Formatado => $"{Value[..2]}.{Value[2..5]}.{Value[5..8]}/{Value[8..12]}-{Value[12..]}";
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Formatado;
}
