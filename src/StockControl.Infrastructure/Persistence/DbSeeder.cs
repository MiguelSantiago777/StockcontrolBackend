using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StockControl.Application.Interfaces;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Enums;
using StockControl.Domain.Interfaces;
using StockControl.Domain.Repositories;
using StockControl.Domain.ValueObjects;

namespace StockControl.Infrastructure.Persistence;

/// <summary>
/// Cria o usuário Administrador inicial caso a base ainda não tenha nenhum usuário.
/// Credenciais padrão — TROQUE a senha assim que logar pela primeira vez.
/// </summary>
public static class DbSeeder
{
    private const string AdminEmail = "admin@stockcontrol.com";
    private const string AdminSenhaPadrao = "Admin@123";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var usuarioRepository = provider.GetRequiredService<IUsuarioRepository>();
        var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
        var passwordHasher = provider.GetRequiredService<IPasswordHasher>();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        var emailResult = Email.Create(AdminEmail);
        if (emailResult.IsFailure)
        {
            return;
        }

        if (await usuarioRepository.EmailExisteAsync(emailResult.Value))
        {
            return;
        }

        var senhaHashResult = SenhaHash.Create(passwordHasher.Hash(AdminSenhaPadrao));
        if (senhaHashResult.IsFailure)
        {
            return;
        }

        var usuarioResult = Usuario.Criar("Administrador", emailResult.Value, senhaHashResult.Value, PerfilUsuario.Administrador);
        if (usuarioResult.IsFailure)
        {
            return;
        }

        await usuarioRepository.AddAsync(usuarioResult.Value);
        await unitOfWork.SaveChangesAsync();

        logger.LogWarning(
            "Usuário admin criado — email: {Email} | senha: {Senha} — TROQUE a senha assim que possível.",
            AdminEmail, AdminSenhaPadrao);
    }
}
