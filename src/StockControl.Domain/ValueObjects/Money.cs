using StockControl.Domain.Common;

namespace StockControl.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    private Money(decimal amount, string currency) { Amount = amount; Currency = currency; }

    public decimal Amount { get; }
    public string Currency { get; }

    public static readonly Money Zero = new(0, "BRL");

    public static Result<Money> Create(decimal amount, string currency = "BRL")
    {
        if (amount < 0)
            return Result.Failure<Money>(Error.Validation("Money.Negativo", "O valor não pode ser negativo."));
        return new Money(decimal.Round(amount, 2), currency);
    }

    public Money Add(Money other) => CheckCurrency(other, () => new Money(Amount + other.Amount, Currency));
    public Money Subtract(Money other) => CheckCurrency(other, () => new Money(Amount - other.Amount, Currency));
    public Money Multiply(int factor) => new(Amount * factor, Currency);

    private Money CheckCurrency(Money other, Func<Money> op) =>
        Currency == other.Currency ? op()
            : throw new InvalidOperationException("Moedas diferentes.");

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Currency} {Amount:N2}";
}
