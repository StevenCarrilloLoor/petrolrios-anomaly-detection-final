using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace PetrolRios.Api.Middleware;

/// <summary>
/// Middleware global de manejo de excepciones con ProblemDetails.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "No autorizado"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Recurso no encontrado"),
            ArgumentException => (HttpStatusCode.BadRequest, "Solicitud inválida"),
            InvalidOperationException => (HttpStatusCode.Conflict, "Operación inválida"),
            ValidationException => (HttpStatusCode.BadRequest, "Error de validación"),
            _ => (HttpStatusCode.InternalServerError, "Error interno del servidor")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Error no controlado: {Message}", exception.Message);
        else
            _logger.LogWarning("Excepción controlada ({Status}): {Message}", statusCode, exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        if (exception is ValidationException validationEx)
        {
            problemDetails.Extensions["errors"] = validationEx.Errors
                .Select(e => new { e.PropertyName, e.ErrorMessage })
                .ToList();
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
