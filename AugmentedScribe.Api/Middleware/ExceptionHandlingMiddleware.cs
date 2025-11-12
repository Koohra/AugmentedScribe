using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using AugmentedScribe.Domain.Exceptions;

namespace AugmentedScribe.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorTitle, errorDetails) = exception switch
        {
            ValidationException validationException =>
                (HttpStatusCode.BadRequest, "Validation Error", validationException.Message),

            NotFoundException notFoundException =>
                (HttpStatusCode.NotFound, "Not Found", notFoundException.Message),

            UnauthorizedAccessException =>
                (HttpStatusCode.Unauthorized, "Unauthorized", "Authentication is required."),

            _ =>
                (HttpStatusCode.InternalServerError, "Internal Server Error", 
                    "An unexpected error occurred. Please try again later.")
        };
        
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            title = errorTitle,
            status = (int)statusCode,
            detail = errorDetails,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            })
        );
    }
}