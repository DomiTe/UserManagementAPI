using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog 
Log.Logger = new LoggerConfiguration()
    .WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), "logs/log-.json", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure Middleware pipeline
app.UseMiddleware<ErrorHandlingMiddleware>(); // Error-handling first
app.UseMiddleware<AuthenticationMiddleware>(); // Authentication next
app.UseMiddleware<LoggingMiddleware>(); // Logging last

// In-memory dictionary to store users with example data (optimized lookup)
var users = new Dictionary<int, User>
{
    { 1, new User(1, "Alice", 25) },
    { 2, new User(2, "Bob", 30) },
    { 3, new User(3, "Charlie", 35) }
};

// Welcome endpoint
app.MapGet("/", () => "Welcome to the User Management API!");

// Retrieve all users
app.MapGet("/users", () => users.Values);

// Retrieve a specific user by ID
app.MapGet("/users/{id:int}", (int id) =>
{
    try
    {
        if (!users.TryGetValue(id, out var user))
        {
            return Results.NotFound();
        }
        return Results.Ok(user);
    }
    catch (Exception ex)
    {
        // Log exception (if logging is enabled)
        Log.Error(ex, "An error occurred while retrieving the user.");
        return Results.Problem($"An error occurred while retrieving the user: {ex}");
    }
});

// Add a new user with validation
app.MapPost("/users", (User user) =>
{
    // Validate user input
    if (string.IsNullOrWhiteSpace(user.Name))
    {
        return Results.BadRequest("User name cannot be empty.");
    }

    if (user.Name.All(char.IsDigit))
    {
        return Results.BadRequest("User name cannot be numbers.");
    }

    if (user.Age <= 0 || user.Age > 150)
    {
        return Results.BadRequest("Age must be between 0 and 150.");
    }

    var newUser = user with { Id = users.Keys.Max() + 1 }; // Auto-generate new ID
    users[newUser.Id] = newUser;
    return Results.Created($"/users/{newUser.Id}", newUser);
});

// Update an existing user with validation
app.MapPut("/users/{id:int}", (int id, User updatedUser) =>
{
    // Validate user input
    if (string.IsNullOrWhiteSpace(updatedUser.Name))
    {
        return Results.BadRequest("User name cannot be empty.");
    }

    if (updatedUser.Name.All(char.IsDigit))
    {
        return Results.BadRequest("User name cannot be numbers.");
    }

    if (updatedUser.Age <= 0 || updatedUser.Age > 150)
    {
        return Results.BadRequest("Age must be between 0 and 150.");
    }

    try
    {
        if (!users.ContainsKey(id))
        {
            return Results.NotFound();
        }

        users[id] = updatedUser with { Id = id }; // Keep the same ID
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        // Log exception (if logging is enabled)
        Log.Error(ex, "An error occurred while updating the user.");
        return Results.Problem($"An error occurred while updating the user: {ex}");
    }
});

// Delete a user by ID
app.MapDelete("/users/{id:int}", (int id) =>
{
    try
    {
        if (!users.ContainsKey(id))
        {
            return Results.NotFound();
        }

        users.Remove(id);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        // Log exception (if logging is enabled)
        Log.Error(ex, "An error occurred while deleting the user.");
        return Results.Problem($"An error occurred while deleting the user: {ex}");
    }
});

// Enable Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.Run();

// Define the User record **after** the top-level statements
public record User(int Id, string Name, int Age);

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log the incoming request
        Log.Information($"HTTP {context.Request.Method} {context.Request.Path}");

        // Call the next middleware in the pipeline
        await _next(context);

        // Log the outgoing response
        Log.Information($"Response Status: {context.Response.StatusCode}");
    }
}

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Call the next middleware in the pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the error and return a consistent response
            Log.Error($"Error: {ex.Message}");
            Log.Error($"Stack Trace: {ex.StackTrace}");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "Internal server error." });
        }
    }
}

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (string.IsNullOrEmpty(token) || !ValidateToken(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
            return;
        }

        await _next(context);
    }

    private bool ValidateToken(string token)
    {
        // Example validation logic (this should be replaced with actual token validation)
        return token == "valid_token";
    }
}

