using System.Text.Json.Serialization;

namespace StockControl.Application.DTOs;

public sealed class ClienteDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("cpf")]
    public string Cpf { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public EnderecoDto Address { get; set; } = new();

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}
