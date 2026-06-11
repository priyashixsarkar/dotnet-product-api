using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Application.Interfaces;
using Application.Services;

namespace Application.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IItemService, ItemService>();

            return services;
        }
    }
}
