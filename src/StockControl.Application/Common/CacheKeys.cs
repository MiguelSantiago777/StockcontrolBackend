namespace StockControl.Application.Common;

/// <summary>Chaves de cache compartilhadas entre handlers (invalidação centralizada).</summary>
public static class CacheKeys
{
    public const string Produtos = "produtos:lista";
    public const string Dashboard = "dashboard:resumo";
    public const string Categorias = "categorias:lista";
}
