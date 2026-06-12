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
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure DbContext (auto-detect PostgreSQL vs SQL Server)
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
            
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (connectionString.Contains("Host=") || connectionString.Contains("postgresql", StringComparison.OrdinalIgnoreCase) || connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
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
