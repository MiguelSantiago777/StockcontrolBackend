using System.Text.Json.Serialization;

namespace StockControl.Application.DTOs;

public sealed class VeiculoDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("plate")]
    public string Plate { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}
