using Microsoft.Extensions.DependencyInjection;

namespace PetrolRios.Detectors;

public static class DependencyInjection
{
    public static IServiceCollection AddDetectors(this IServiceCollection services)
    {
        return services;
    }
}
