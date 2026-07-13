using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using StockControl.API.Middlewares;
using StockControl.Application;
using StockControl.Infrastructure;
using StockControl.Infrastructure.Persistence;
using StockControl.Infrastructure.Persistence.Context;
using StockControl.Infrastructure.SignalR;

namespace StockControl.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, config) => config
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/stockcontrol-.log", rollingInterval: RollingInterval.Day));

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "StockControl API",
                Version = "v1",
                Description = "API de controle de estoque — DDD + Clean Architecture"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Informe apenas o token JWT (sem o prefixo Bearer)",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        builder.Services.AddCors(options => options.AddPolicy("Default", policy =>
            policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                    ?? new[] { "http://localhost:3000" })
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));

        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseSerilogRequestLogging();

        // Swagger habilitado em todos os ambientes — a API inicia direto nele
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "StockControl API v1");
            options.DocumentTitle = "StockControl API";
        });

        app.UseCors("Default");
        app.UseStaticFiles(); // serve /uploads/... (imagens de produtos)
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHub<StockHub>(StockHub.Route);
        app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));
        app.MapGet("/", () => Results.Redirect("/swagger"));

        // Aplica as migrations pendentes automaticamente ao subir (essencial no Docker)
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }

        await DbSeeder.SeedAsync(app.Services);

        await app.RunAsync();
    }
}
