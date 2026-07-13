using System.Text.Json.Serialization;

namespace StockControl.Application.DTOs;

/// <summary>
/// Propriedades em português (convenção do projeto), mas serializadas em inglês
/// via JsonPropertyName para bater com o contrato esperado pelo front (stockcontrol-web).
/// </summary>
public sealed class ProdutoDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Descricao { get; set; }

    [JsonPropertyName("code")]
    public string Codigo { get; set; } = string.Empty;

    [JsonPropertyName("barcode")]
    public string? CodigoBarras { get; set; }

    [JsonPropertyName("price")]
    public decimal Preco { get; set; }

    [JsonPropertyName("stock")]
    public int Estoque { get; set; }

    [JsonPropertyName("minStock")]
    public int EstoqueMinimo { get; set; }

    [JsonPropertyName("categoryId")]
    public Guid CategoriaId { get; set; }

    [JsonPropertyName("supplierId")]
    public Guid? FornecedorId { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImagemUrl { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("belowMinStock")]
    public bool AbaixoDoMinimo { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}
