using System.Text.Json.Serialization;

namespace StockControl.Application.DTOs;

public sealed class EntregadorDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("cpf")]
    public string Cpf { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("lastPosition")]
    public CoordenadaDto? LastPosition { get; set; }

    [JsonPropertyName("positionUpdatedAt")]
    public DateTime? PositionUpdatedAt { get; set; }

    [JsonPropertyName("vehicleId")]
    public Guid? VehicleId { get; set; }

    [JsonPropertyName("vehiclePlate")]
    public string? VehiclePlate { get; set; }

    [JsonPropertyName("vehicleType")]
    public string? VehicleType { get; set; }

    [JsonPropertyName("vehicleModel")]
    public string? VehicleModel { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}

public sealed class CoordenadaDto
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}
