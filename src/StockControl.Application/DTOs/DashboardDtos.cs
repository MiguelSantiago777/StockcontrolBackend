using System.Text.Json.Serialization;

namespace StockControl.Application.DTOs;

public sealed class DashboardSummaryDto
{
    [JsonPropertyName("totalProducts")]
    public int TotalProducts { get; set; }

    [JsonPropertyName("lowStockProducts")]
    public int LowStockProducts { get; set; }

    [JsonPropertyName("outOfStockProducts")]
    public int OutOfStockProducts { get; set; }

    [JsonPropertyName("totalCustomers")]
    public int TotalCustomers { get; set; }

    [JsonPropertyName("totalOrders")]
    public int TotalOrders { get; set; }

    [JsonPropertyName("activeDeliveries")]
    public int ActiveDeliveries { get; set; }

    [JsonPropertyName("totalUsers")]
    public int TotalUsers { get; set; }

    [JsonPropertyName("totalMovements")]
    public int TotalMovements { get; set; }
}

public sealed class DatasetDto
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("values")]
    public List<int> Values { get; set; } = [];
}

public sealed class MonthlySeriesDto
{
    [JsonPropertyName("labels")]
    public List<string> Labels { get; set; } = [];

    [JsonPropertyName("datasets")]
    public List<DatasetDto> Datasets { get; set; } = [];
}

public sealed class TopProductsDto
{
    [JsonPropertyName("labels")]
    public List<string> Labels { get; set; } = [];

    [JsonPropertyName("values")]
    public List<int> Values { get; set; } = [];
}
