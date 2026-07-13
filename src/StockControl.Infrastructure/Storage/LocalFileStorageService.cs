using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using StockControl.Application.Interfaces;

namespace StockControl.Infrastructure.Storage;

/// <summary>
/// Armazena arquivos em disco, dentro de wwwroot, servidos como estáticos.
/// Simples e suficiente para desenvolvimento — troque por S3/Azure Blob em produção.
/// </summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalFileStorageService(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
    {
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> SalvarAsync(
        string pasta,
        string nomeArquivo,
        Stream conteudo,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var webRoot = _environment.WebRootPath
            ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

        var diretorio = Path.Combine(webRoot, "uploads", pasta);
        Directory.CreateDirectory(diretorio);

        var caminhoCompleto = Path.Combine(diretorio, nomeArquivo);

        await using (var arquivo = File.Create(caminhoCompleto))
        {
            await conteudo.CopyToAsync(arquivo, cancellationToken);
        }

        var request = _httpContextAccessor.HttpContext?.Request;
        var baseUrl = request is not null
            ? $"{request.Scheme}://{request.Host}"
            : string.Empty;

        return $"{baseUrl}/uploads/{pasta}/{nomeArquivo}";
    }
}
