using System.Reflection;
using Application.Interfaces;
using Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddAutoMapper(cfg => cfg.AddProfile<Application.Mapping.MappingProfile>());
        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IItemService, ItemService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
