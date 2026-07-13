using System.Text.RegularExpressions;
using StockControl.Domain.Common;

namespace StockControl.Domain.ValueObjects;

public sealed partial class Email : ValueObject
{
    private Email(string value) => Value = value;
    public string Value { get; }

    public static Result<Email> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Email>(Error.Validation("Email.Vazio", "O e-mail é obrigatório."));

        email = email.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(email))
            return Result.Failure<Email>(Error.Validation("Email.Invalido", "Formato de e-mail inválido."));

        return new Email(email);
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
