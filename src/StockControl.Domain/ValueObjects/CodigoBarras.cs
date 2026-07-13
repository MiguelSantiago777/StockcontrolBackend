using StockControl.Domain.Common;

namespace StockControl.Domain.ValueObjects;

public sealed class CodigoBarras : ValueObject
{
    private CodigoBarras(string value) => Value = value;
    public string Value { get; }

    /// <summary>Aceita EAN-8, EAN-13 e ITF-14.</summary>
    public static Result<CodigoBarras> Create(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return Result.Failure<CodigoBarras>(Error.Validation("CodigoBarras.Vazio", "O código de barras é obrigatório."));

        var digits = new string(codigo.Where(char.IsDigit).ToArray());

        if (digits.Length is not (8 or 13 or 14))
            return Result.Failure<CodigoBarras>(Error.Validation("CodigoBarras.Invalido", "Código de barras inválido."));

        return new CodigoBarras(digits);
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value;
}
