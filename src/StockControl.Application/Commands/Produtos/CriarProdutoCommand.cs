using System.Text.Json.Serialization;
using MediatR;
using StockControl.Application.DTOs;
using StockControl.Domain.Common;

namespace StockControl.Application.Commands.Produtos;

public sealed class CriarProdutoCommand : IRequest<Result<ProdutoDto>>
{
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
    public int EstoqueInicial { get; set; }

    [JsonPropertyName("minStock")]
    public int EstoqueMinimo { get; set; }

    [JsonPropertyName("categoryId")]
    public Guid CategoriaId { get; set; }

    [JsonPropertyName("supplierId")]
    public Guid? FornecedorId { get; set; }
}
