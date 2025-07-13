using System.Text;
using HealthServices.Api.Configurations;
using HealthServices.Application.Services;
using HealthServices.Infrastructure.DbContexts;
using HealthServices.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Only set URLs for local development (not in Docker)
if (
    !builder.Environment.IsProduction()
    && Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true"
)
{
    builder.WebHost.UseUrls("http://localhost:5292", "https://localhost:7292");
}

// Configuration binding
var authSettings = builder
    .Configuration.GetSection(AuthenticationSettings.SectionName)
    .Get<AuthenticationSettings>();
builder.Services.Configure<AuthenticationSettings>(
    builder.Configuration.GetSection(AuthenticationSettings.SectionName)
);

// Database configuration
builder.Services.AddDbContext<HealthServicesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Register application services
builder.Services.AddScoped<
    IAuthenticationService,
    HealthServices.Infrastructure.Services.AuthenticationService
>();

// Authentication & Authorization
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = authSettings?.Jwt.Issuer,
            ValidAudience = authSettings?.Jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(authSettings?.Jwt.Key ?? "")
            ),
        };
    })
    .AddGoogle(options =>
    {
        options.ClientId = authSettings?.Google.ClientId ?? "";
        options.ClientSecret = authSettings?.Google.ClientSecret ?? "";
        options.CallbackPath = "/signin-google";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("DoctorOnly", policy => policy.RequireRole("Doctor"));
    options.AddPolicy("PatientOnly", policy => policy.RequireRole("Patient"));
    options.AddPolicy("DoctorOrAdmin", policy => policy.RequireRole("Doctor", "Admin"));
});

// Add controllers
builder.Services.AddControllers();

// Add services for Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Health Services API", Version = "v1" });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition(
        "Bearer",
        new()
        {
            Description =
                @"JWT Authorization header using the Bearer scheme. 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
        }
    );

    c.AddSecurityRequirement(
        new()
        {
            {
                new()
                {
                    Reference = new()
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );

    // Enable XML comments for better API documentation (optional)
    try
    {
        c.IncludeXmlComments(
            Path.Combine(AppContext.BaseDirectory, "HealthServices.Api.xml"),
            true
        );
    }
    catch
    {
        // XML documentation file not found - this is optional
    }
});

var app = builder.Build();

// Apply database migrations automatically
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<HealthServicesDbContext>();
    try
    {
        dbContext.Database.Migrate();
        app.Logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while applying database migrations.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

var summaries = new[]
{
    "Freezing",
    "Bracing",
    "Chilly",
    "Cool",
    "Mild",
    "Warm",
    "Balmy",
    "Hot",
    "Sweltering",
    "Scorching",
};

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck");

app.MapGet(
        "/weatherforecast",
        () =>
        {
            var forecast = Enumerable
                .Range(1, 5)
                .Select(index => new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();
            return forecast;
        }
    )
    .WithName("GetWeatherForecast")
    .RequireAuthorization(); // Require authentication for this endpoint

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
