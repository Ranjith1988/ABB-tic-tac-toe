using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using TicTacToe.Api.Infrastructure;
using TicTacToe.Api.Middleware;
using TicTacToe.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tic Tac Toe API",
        Version = "v1",
        Description = "REST API for a browser-based Tic Tac Toe game."
    });
});

// The exercise explicitly permits in-memory state. The store is singleton so all requests
// in the local application share the same game sessions and session-level scoreboard.
builder.Services.AddSingleton<IGameStore, InMemoryGameStore>();
builder.Services.AddSingleton<IComputerMoveStrategy, BasicComputerMoveStrategy>();
builder.Services.AddSingleton<IGameService, GameService>();

const string CorsPolicy = "Frontend";
builder.Services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
{
    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").GetChildren()
        .Select(section => section.Value)
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .Cast<string>()
        .ToArray();
    if (origins.Length == 0) origins = ["http://localhost:4200"];
    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
}));

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(CorsPolicy);
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" })).WithTags("Health");
app.MapControllers();
app.Run();

public partial class Program { }
