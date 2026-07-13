namespace StockControl.Application.Interfaces;

public interface IFileStorageService
{
    /// <summary>Salva o arquivo e retorna a URL pública para acessá-lo.</summary>
    Task<string> SalvarAsync(
        string pasta,
        string nomeArquivo,
        Stream conteudo,
        string contentType,
        CancellationToken cancellationToken = default);
}
