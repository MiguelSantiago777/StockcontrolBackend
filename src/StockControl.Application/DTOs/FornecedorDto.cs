using System.Text.Json.Serialization;

namespace StockControl.Application.DTOs;

public sealed class FornecedorDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("companyName")]
    public string RazaoSocial { get; set; } = string.Empty;

    [JsonPropertyName("tradeName")]
    public string? NomeFantasia { get; set; }

    [JsonPropertyName("cnpj")]
    public string Cnpj { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public EnderecoDto Address { get; set; } = new();

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}

public sealed class EnderecoDto
{
    [JsonPropertyName("street")]
    public string Street { get; set; } = string.Empty;

    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("complement")]
    public string? Complement { get; set; }

    [JsonPropertyName("neighborhood")]
    public string Neighborhood { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("zipCode")]
    public string ZipCode { get; set; } = string.Empty;
}
