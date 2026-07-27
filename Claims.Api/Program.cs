using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using Claims.Api.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add Logging & Infrastructure Services
builder.Services.AddLogging();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Claims API (GitOps Production Grade)",
        Version = "v1",
        Description = "Production-ready enterprise API service deployed via GitOps."
    });
});

// Native .NET Health Checks Engine
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Application is healthy"), tags: new[] { "live" });

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Swagger UI Configuration
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Claims API v1");
    c.RoutePrefix = "swagger";
});

// Environment / Application Metadata
var appVersion = Environment.GetEnvironmentVariable("APP_VERSION")
                 ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                 ?? "1.0.1-dev";

var environmentName = builder.Environment.EnvironmentName;

logger.LogInformation("Starting Claims API Service. Version: {Version}, Environment: {Env}", appVersion, environmentName);

// Root Status Endpoint
app.MapGet("/", (ILogger<Program> log) =>
{
    log.LogInformation("Root endpoint requested.");
    return Results.Ok(new
    {
        Status = "Online",
        Service = "Claims API",
        Version = appVersion,
        Environment = environmentName,
        Timestamp = DateTime.UtcNow
    });
})
.WithName("GetRootStatus")
.Produces(StatusCodes.Status200OK);

// Custom Structured Health Check Endpoint using HealthCheckResponse DTO
app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var response = new HealthCheckResponse(
            Status: report.Status.ToString(),
            TotalDuration: report.TotalDuration.ToString(),
            Entries: report.Entries.ToDictionary(
                entry => entry.Key,
                entry => entry.Value.Status.ToString()
            )
        );

        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
});

// Business Domain Endpoint: Claims
app.MapGet("/api/claims", (ILogger<Program> log) =>
{
    log.LogInformation("Fetching all claims");

    var claims = new List<ClaimDto>
    {
        new(101, "Acme Corp", 1500.00m, "Submitted", DateTime.UtcNow.AddDays(-2)),
        new(102, "Globex Inc", 4200.50m, "Approved", DateTime.UtcNow.AddDays(-1)),
        new(103, "Initech LLC", 200.50m, "Completed", DateTime.UtcNow)
    };

    return Results.Ok(claims);
})
.WithName("GetClaims")
.Produces<IEnumerable<ClaimDto>>(StatusCodes.Status200OK);

app.Run();