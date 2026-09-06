using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using TicTacToe.Api.Middleware;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Tests;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task SuccessfulRequest_PassesThrough()
    {
        var middleware = new ExceptionHandlingMiddleware(context =>
        {
            context.Response.StatusCode = StatusCodes.Status204NoContent;
            return Task.CompletedTask;
        }, NullLogger<ExceptionHandlingMiddleware>.Instance);

        var context = new DefaultHttpContext();
        await middleware.Invoke(context);

        Assert.Equal(StatusCodes.Status204NoContent, context.Response.StatusCode);
    }

    [Fact]
    public async Task DomainException_ReturnsBadRequestApiError()
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw new DomainException("BAD_MOVE", "Invalid move."), NullLogger<ExceptionHandlingMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.Invoke(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Contains("BAD_MOVE", await ReadBody(context));
    }

    [Fact]
    public async Task UnexpectedException_ReturnsInternalError()
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw new InvalidOperationException("boom"), NullLogger<ExceptionHandlingMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.Invoke(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Contains("INTERNAL_ERROR", await ReadBody(context));
    }

    private static async Task<string> ReadBody(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}