using Microsoft.Extensions.DependencyInjection;
using PetrolRios.Application.Interfaces;
using PetrolRios.Application.Security;

namespace PetrolRios.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<ITotpService, TotpService>();
        return services;
    }
}
