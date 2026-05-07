using Ambev.DeveloperEvaluation.Domain.Exceptions;
using FluentValidation;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error");
            await WriteError(context, 400, "ValidationError", "Invalid input data",
                string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)));
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain error");
            var code = ex.Message.Contains("not found") ? 404 : 422;
            await WriteError(context, code, "DomainError", ex.Message, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteError(context, 500, "InternalError", "An unexpected error occurred", ex.Message);
        }
    }

    private static async Task WriteError(HttpContext ctx, int status, string type, string error, string detail)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(new { type, error, detail }));
    }
}
