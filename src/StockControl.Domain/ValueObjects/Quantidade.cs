using StockControl.Domain.Common;

namespace StockControl.Domain.ValueObjects;

public sealed class Quantidade : ValueObject
{
    private Quantidade(int value) => Value = value;
    public int Value { get; }

    public static readonly Quantidade Zero = new(0);

    public static Result<Quantidade> Create(int value)
    {
        if (value < 0)
            return Result.Failure<Quantidade>(Error.Validation("Quantidade.Negativa", "A quantidade não pode ser negativa."));
        return new Quantidade(value);
    }

    public Result<Quantidade> Somar(Quantidade outra) => Create(Value + outra.Value);
    public Result<Quantidade> Subtrair(Quantidade outra) => Create(Value - outra.Value);

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value.ToString();
}
