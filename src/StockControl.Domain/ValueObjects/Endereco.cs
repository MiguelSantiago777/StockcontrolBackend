using StockControl.Domain.Common;

namespace StockControl.Domain.ValueObjects;

public sealed class Endereco : ValueObject
{
    private Endereco(string logradouro, string numero, string? complemento,
        string bairro, string cidade, string uf, string cep)
    {
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        Cidade = cidade;
        Uf = uf;
        Cep = cep;
    }

    public string Logradouro { get; }
    public string Numero { get; }
    public string? Complemento { get; }
    public string Bairro { get; }
    public string Cidade { get; }
    public string Uf { get; }
    public string Cep { get; }

    public static Result<Endereco> Create(string? logradouro, string? numero, string? complemento,
        string? bairro, string? cidade, string? uf, string? cep)
    {
        if (string.IsNullOrWhiteSpace(logradouro))
            return Result.Failure<Endereco>(Error.Validation("Endereco.Logradouro", "Logradouro é obrigatório."));
        if (string.IsNullOrWhiteSpace(cidade))
            return Result.Failure<Endereco>(Error.Validation("Endereco.Cidade", "Cidade é obrigatória."));
        if (string.IsNullOrWhiteSpace(uf) || uf.Trim().Length != 2)
            return Result.Failure<Endereco>(Error.Validation("Endereco.Uf", "UF inválida."));

        var cepDigits = new string((cep ?? "").Where(char.IsDigit).ToArray());
        if (cepDigits.Length != 8)
            return Result.Failure<Endereco>(Error.Validation("Endereco.Cep", "CEP inválido."));

        return new Endereco(logradouro.Trim(), numero?.Trim() ?? "S/N", complemento?.Trim(),
            bairro?.Trim() ?? string.Empty, cidade.Trim(), uf.Trim().ToUpperInvariant(), cepDigits);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Logradouro; yield return Numero; yield return Complemento;
        yield return Bairro; yield return Cidade; yield return Uf; yield return Cep;
    }
}
