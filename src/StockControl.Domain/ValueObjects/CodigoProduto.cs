using StockControl.Domain.Common;

namespace StockControl.Domain.ValueObjects;

public sealed class CodigoProduto : ValueObject
{
    private CodigoProduto(string value) => Value = value;
    public string Value { get; }

    public static Result<CodigoProduto> Create(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return Result.Failure<CodigoProduto>(Error.Validation("CodigoProduto.Vazio", "O código do produto é obrigatório."));

        codigo = codigo.Trim().ToUpperInvariant();

        if (codigo.Length is < 3 or > 30)
            return Result.Failure<CodigoProduto>(Error.Validation("CodigoProduto.Tamanho", "O código deve ter entre 3 e 30 caracteres."));

        return new CodigoProduto(codigo);
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value;
}
