using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Identity;

namespace Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        // Converts postgres://user:pass@host:port/db URL to Npgsql key=value format
        private static string ConvertPostgresUrlToConnectionString(string url)
        {
            var uri = new Uri(url);
            var userInfo = uri.UserInfo.Split(':');
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');
            var username = userInfo[0];
            var password = userInfo.Length > 1 ? userInfo[1] : "";
            return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure DbContext (auto-detect PostgreSQL vs SQL Server)
            var rawConnectionString = configuration.GetConnectionString("DefaultConnection") ?? "";

            // Convert postgres:// URL format (used by Render) to Npgsql key=value format
            var connectionString = (rawConnectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
                                    rawConnectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
                ? ConvertPostgresUrlToConnectionString(rawConnectionString)
                : rawConnectionString;

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (connectionString.Contains("Host=") || connectionString.Contains("postgresql", StringComparison.OrdinalIgnoreCase))
                {
                    options.UseNpgsql(connectionString,
                        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
                }
                else
                {
                    options.UseSqlServer(connectionString,
                        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
                }
            });

            // Register repositories and UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Configure JWT settings
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            // Register Identity services
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IIdentityService, IdentityService>();

            // Register Services
            services.AddTransient<IDateTime, Services.DateTimeService>();

            return services;
        }
    }
}
