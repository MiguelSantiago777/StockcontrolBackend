using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StockControl.Application.Interfaces;
using StockControl.Domain.Interfaces;
using StockControl.Domain.Repositories;
using StockControl.Infrastructure.Authentication;
using StockControl.Infrastructure.Cache;
using StockControl.Infrastructure.Identity;
using StockControl.Infrastructure.Persistence.Context;
using StockControl.Infrastructure.Persistence.Repositories;
using StockControl.Infrastructure.Services;
using StockControl.Infrastructure.SignalR;
using StockControl.Infrastructure.Storage;

namespace StockControl.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Persistence
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IPedidoRepository, PedidoRepository>();
        services.AddScoped<IMovimentacaoRepository, MovimentacaoRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IFornecedorRepository, FornecedorRepository>();
        services.AddScoped<IEntregadorRepository, EntregadorRepository>();

        // Cache (Redis)
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "stockcontrol:";
        });
        services.AddScoped<ICacheService, RedisCacheService>();

        // Identity / Auth
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IJwtService, JwtService>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                // Permite JWT via query string para o SignalR
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken) &&
                            context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                            context.Token = accessToken;
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.Administrador, p => p.RequireRole("Administrador"))
            .AddPolicy(Policies.GerenciaEstoque, p => p.RequireRole("Administrador", "Estoquista"))
            .AddPolicy(Policies.Entregas, p => p.RequireRole("Administrador", "Entregador"))
            .AddPolicy(Policies.Leitura, p => p.RequireRole("Administrador", "Estoquista", "Entregador", "Visualizador"));

        // SignalR
        services.AddSignalR();
        services.AddScoped<INotificacaoService, SignalRNotificacaoService>();

        return services;
    }

    public static class Policies
    {
        public const string Administrador = "Administrador";
        public const string GerenciaEstoque = "GerenciaEstoque";
        public const string Entregas = "Entregas";
        public const string Leitura = "Leitura";
    }
}
