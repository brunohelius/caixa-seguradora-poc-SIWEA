using CaixaSeguradora.Core.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace CaixaSeguradora.Api.Middleware;

/// <summary>
/// Global Exception Handler Middleware
/// Catches all unhandled exceptions and returns standardized error responses
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = Activity.Current?.Id ?? context.TraceIdentifier;

        _logger.LogError(exception, "Unhandled exception occurred. CorrelationId: {CorrelationId}", correlationId);

        var (statusCode, errorCode, message, details) = exception switch
        {
            ClaimNotFoundException cnfe => (
                HttpStatusCode.NotFound,
                cnfe.ErrorCode,
                cnfe.Message,
                new List<string>()
            ),

            ValidationException ve => (
                HttpStatusCode.BadRequest,
                "VALIDACAO_FALHOU",
                "Um ou mais erros de validação ocorreram",
                ve.Errors.Select(e => e.ErrorMessage).ToList()
            ),

            DbUpdateConcurrencyException => (
                HttpStatusCode.Conflict,
                "CONFLITO_CONCORRENCIA",
                "O registro foi modificado por outro usuário. Por favor, recarregue e tente novamente.",
                new List<string>()
            ),

            ArgumentNullException ane => (
                HttpStatusCode.BadRequest,
                "ARGUMENTO_NULO",
                $"Parâmetro obrigatório '{ane.ParamName}' não foi fornecido",
                new List<string>()
            ),

            ArgumentException ae => (
                HttpStatusCode.BadRequest,
                "ARGUMENTO_INVALIDO",
                ae.Message,
                new List<string>()
            ),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "NAO_AUTORIZADO",
                "Você não tem permissão para acessar este recurso",
                new List<string>()
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                "ERRO_INTERNO",
                "Ocorreu um erro interno no servidor. Tente novamente mais tarde.",
                new List<string>()
            )
        };

        var errorResponse = new
        {
            sucesso = false,
            codigoErro = errorCode,
            mensagem = message,
            detalhes = details,
            timestamp = DateTime.UtcNow,
            traceId = correlationId
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(errorResponse, options));
    }
}

/// <summary>
/// Extension method for registering middleware
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
