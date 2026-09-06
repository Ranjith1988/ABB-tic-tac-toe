using System.Text.Json;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException ex)
        {
            logger.LogInformation("Domain validation failure: {Code}", ex.Code);
            await WriteError(context, StatusCodes.Status400BadRequest, new ApiError(ex.Code, ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception processing request {Path}", context.Request.Path);
            await WriteError(context, StatusCodes.Status500InternalServerError,
                new ApiError("INTERNAL_ERROR", "An unexpected error occurred."));
        }
    }

    private static async Task WriteError(HttpContext context, int status, ApiError error)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(error));
    }
}
