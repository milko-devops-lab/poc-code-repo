var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger in all environments for PoC demo visibility
app.UseSwagger();
app.UseSwaggerUI();

var appVersion = Environment.GetEnvironmentVariable("APP_VERSION") ?? "1.0.0-dev";
var environmentName = builder.Environment.EnvironmentName;

app.MapGet("/", () => Results.Ok(new 
{ 
    Status = "Online", 
    Service = "Claims API (GitOps Demo)", 
    Version = appVersion,
    Environment = environmentName,
    Timestamp = DateTime.UtcNow 
}));

app.MapGet("/healthz", () => Results.Ok(new { Status = "Healthy" }));

app.MapGet("/api/claims", () => Results.Ok(new[]
{
    new { Id = 101, Customer = "Acme Corp", Amount = 1500.00, Status = "Submitted" },
    new { Id = 102, Customer = "Globex Inc", Amount = 4200.50, Status = "Approved" }
}));

app.Run();