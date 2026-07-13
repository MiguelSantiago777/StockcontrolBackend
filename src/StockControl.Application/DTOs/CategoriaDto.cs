using System.Text.Json.Serialization;

namespace StockControl.Application.DTOs;

public sealed class CategoriaDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Descricao { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}
