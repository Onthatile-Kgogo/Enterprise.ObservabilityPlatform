using Enterprise.Observability.AspNetCore.Context;
using Enterprise.Observability.Core.Abstractions;
using Enterprise.Observability.Tracing.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Observability.AspNetCore.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddObservability(this IServiceCollection services)
    {
        services.AddSingleton<IObservabilityContext, ObservabilityContextAccessor>();
        services.AddObservabilityTracing();
        return services;
    }
}