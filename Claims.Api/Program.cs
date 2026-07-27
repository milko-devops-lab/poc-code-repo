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

// Root Dashboard Endpoint for GitOps Visual Testing
app.MapGet("/", (ILogger<Program> log) =>
{
    log.LogInformation("Root dashboard requested.");

    // Динамичен цвят за бързо разпознаване на средата в OpenShift Route
    var themeColor = environmentName.ToLower() switch
    {
        "development" => "#2563eb", // Син цвят за DEV
        "test" => "#d97706", // Оранжев цвят за TEST
        "production" => "#16a34a", // Зелен цвят за PROD
        _ => "#4b5563"
    };

    // Текстът, който лесно подлежи на промяна при Git commit за демонстрация
    var featureText = "🚀 GitOps Demo: Automatic Sync Enabled!";

    var htmlContent = $"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Claims API - GitOps Dashboard</title>
            <style>
                body {{font - family: system-ui, -apple-system, sans-serif; background: #0f172a; color: #f8fafc; display: flex; justify-content: center; align-items: center; height: 100vh; margin: 0; }}
                .card {{background: #1e293b; padding: 2.5rem; border-radius: 12px; box-shadow: 0 10px 25px rgba(0,0,0,0.5); text-align: center; max-width: 500px; width: 100%; border-top: 6px solid {themeColor}; }}
                .badge {{background: {themeColor}; color: white; padding: 0.35rem 0.8rem; border-radius: 9999px; font-weight: bold; font-size: 0.85rem; text-transform: uppercase; display: inline-block; margin-bottom: 1rem; }}
                h1 {{font - size: 1.8rem; margin: 0 0 0.5rem 0; }}
                p.feature {{color: #38bdf8; font-size: 1.1rem; font-weight: 600; margin: 1.5rem 0; padding: 0.75rem; background: #0f172a; border-radius: 8px; border: 1px dashed #38bdf8; }}
                .meta {{font - size: 0.85rem; color: #94a3b8; text-align: left; margin-top: 1.5rem; border-top: 1px solid #334155; padding-top: 1rem; line-height: 1.6; }}
                a {{color: #38bdf8; text-decoration: none; font-weight: bold; }}
                a:hover {{text - decoration: underline; }}
            </style>
        </head>
        <body>
            <div class="card">
                <span class="badge">{environmentName}</span>
                <h1>Claims API Service</h1>
                <p class="feature">{featureText}</p>
                <p><a href="/swagger">📚 Open Swagger API Docs</a></p>
                <div class="meta">
                    <div><strong>Image Tag / Version:</strong> <code>{appVersion}</code></div>
                    <div><strong>Status:</strong> Healthy 🟢</div>
                    <div><strong>Last Sync:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</div>
                </div>
            </div>
        </body>
        </html>
        """;

    return Results.Content(htmlContent, "text/html");
})
.WithName("GetRootStatus");

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