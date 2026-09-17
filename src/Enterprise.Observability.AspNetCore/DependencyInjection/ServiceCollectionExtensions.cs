using Enterprise.Observability.Tracing.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Observability.AspNetCore.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddObservability(this IServiceCollection services)
    {
        services.AddObservabilityTracing();
        return services;
    }
}