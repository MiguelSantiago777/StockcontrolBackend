using System.Text.RegularExpressions;
using StockControl.Domain.Common;

namespace StockControl.Domain.ValueObjects;

public sealed partial class Placa : ValueObject
{
    private Placa(string value) => Value = value;
    public string Value { get; }

    public static Result<Placa> Create(string? placa)
    {
        if (string.IsNullOrWhiteSpace(placa))
            return Result.Failure<Placa>(Error.Validation("Placa.Vazia", "A placa é obrigatória."));

        var normalizada = placa.Trim().ToUpperInvariant().Replace("-", string.Empty);

        if (!FormatoAntigoRegex().IsMatch(normalizada) && !FormatoMercosulRegex().IsMatch(normalizada))
            return Result.Failure<Placa>(Error.Validation("Placa.Invalida", "Placa inválida (use o formato ABC1234 ou ABC1D23)."));

        return new Placa(normalizada);
    }

    public string Formatada => Value.Length == 7 ? $"{Value[..3]}-{Value[3..]}" : Value;

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Formatada;

    [GeneratedRegex("^[A-Z]{3}[0-9]{4}$")]
    private static partial Regex FormatoAntigoRegex();

    [GeneratedRegex("^[A-Z]{3}[0-9][A-Z][0-9]{2}$")]
    private static partial Regex FormatoMercosulRegex();
}
