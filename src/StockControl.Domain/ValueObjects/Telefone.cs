using StockControl.Domain.Common;

namespace StockControl.Domain.ValueObjects;

public sealed class Telefone : ValueObject
{
    private Telefone(string ddd, string numero) { Ddd = ddd; Numero = numero; }

    public string Ddd { get; }
    public string Numero { get; }

    public static Result<Telefone> Create(string? telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            return Result.Failure<Telefone>(Error.Validation("Telefone.Vazio", "O telefone é obrigatório."));

        var digits = new string(telefone.Where(char.IsDigit).ToArray());

        if (digits.Length is < 10 or > 11)
            return Result.Failure<Telefone>(Error.Validation("Telefone.Invalido", "Telefone inválido."));

        return new Telefone(digits[..2], digits[2..]);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Ddd;
        yield return Numero;
    }

    public override string ToString() => $"({Ddd}) {Numero}";
}
