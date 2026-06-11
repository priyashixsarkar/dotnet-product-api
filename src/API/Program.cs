using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Asp.Versioning;
using API.Filters;
using API.Middleware;
using API.Extensions;
using Application.DTOs;
using Application.Extensions;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Extensions;
using Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog via Infrastructure
Infrastructure.Logging.SerilogConfiguration.ConfigureLogging(builder);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

// Configure API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Register Clean Architecture layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Configure JWT Authentication
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
var jwtSettings = jwtSettingsSection.Get<JwtSettings>() ?? new JwtSettings();
var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // Remove delay of token when expire
    };
});

builder.Services.AddEndpointsApiExplorer();

// Configure Swagger via Extension
builder.Services.AddSwaggerConfiguration();

// Configure Response Compression via Extension
builder.Services.AddResponseCompressionConfiguration();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Enable Swagger globally (development and production) for assessment testing
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API v1");
});

// Global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Enable Response Compression
app.UseResponseCompression();

// Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "no-referrer");
    await next();
});

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Database Migration and Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        logger.LogInformation("Applying pending database migrations...");
        await context.Database.EnsureCreatedAsync();

        var identityService = services.GetRequiredService<IIdentityService>();

        // Seed Admin User
        var adminExists = await context.Users.AnyAsync(u => u.Username.ToLower() == "admin");
        if (!adminExists)
        {
            await identityService.RegisterAsync(new RegisterRequest
            {
                Username = "admin",
                Password = "AdminPassword123!",
                Role = "Admin"
            });
            logger.LogInformation("Seeded Admin user (username: 'admin', password: 'AdminPassword123!').");
        }

        // Seed Standard User
        var userExists = await context.Users.AnyAsync(u => u.Username.ToLower() == "user");
        if (!userExists)
        {
            await identityService.RegisterAsync(new RegisterRequest
            {
                Username = "user",
                Password = "UserPassword123!",
                Role = "User"
            });
            logger.LogInformation("Seeded Standard user (username: 'user', password: 'UserPassword123!').");
        }

        // Seed Products and Items if empty
        if (!await context.Products.AnyAsync())
        {
            var p1 = new Product { ProductName = "Wireless Headphones", CreatedBy = "System", CreatedOn = DateTime.UtcNow };
            var p2 = new Product { ProductName = "Mechanical Keyboard", CreatedBy = "System", CreatedOn = DateTime.UtcNow };
            var p3 = new Product { ProductName = "UltraWide Monitor", CreatedBy = "System", CreatedOn = DateTime.UtcNow };

            context.Products.AddRange(p1, p2, p3);
            await context.SaveChangesAsync();

            context.Items.AddRange(
                new Item { ProductId = p1.Id, Quantity = 120 },
                new Item { ProductId = p2.Id, Quantity = 45 },
                new Item { ProductId = p3.Id, Quantity = 15 }
            );
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded default products and items.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

app.Run();

public partial class Program { }
