using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StockControl.Domain.Exceptions;

namespace StockControl.API.Middlewares;

public sealed class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode statusCode;
        string title;
        List<object> errors = new();

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                title = "Erro de validação";
                errors = validationException.Errors
                    .Select(failure => (object)new { campo = failure.PropertyName, mensagem = failure.ErrorMessage })
                    .ToList();
                break;

            case DomainException domainException:
                statusCode = HttpStatusCode.UnprocessableEntity;
                title = domainException.Message;
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                title = "Não autorizado";
                break;

            case DbUpdateConcurrencyException:
                statusCode = HttpStatusCode.Conflict;
                title = "O registro foi modificado por outro usuário. Recarregue e tente novamente.";
                break;

            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                title = "Recurso não encontrado";
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                title = "Erro interno do servidor";
                break;
        }

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Erro não tratado: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Exceção tratada ({StatusCode}): {Message}", (int)statusCode, exception.Message);
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problem = new
        {
            type = $"https://httpstatuses.com/{(int)statusCode}",
            title,
            status = (int)statusCode,
            traceId = context.TraceIdentifier,
            errors
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }
}
