using StockControl.Domain.Common;

namespace StockControl.Domain.ValueObjects;

public sealed class SenhaHash : ValueObject
{
    private SenhaHash(string value) => Value = value;
    public string Value { get; }

    /// <summary>Cria a partir de um hash já calculado (ex.: BCrypt, na Infrastructure).</summary>
    public static Result<SenhaHash> Create(string? hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return Result.Failure<SenhaHash>(Error.Validation("Senha.HashVazio", "Hash de senha inválido."));
        return new SenhaHash(hash);
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
}
