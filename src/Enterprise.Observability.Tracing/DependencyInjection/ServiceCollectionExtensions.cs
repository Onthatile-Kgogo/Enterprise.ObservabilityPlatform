using Enterprise.Observability.Core.Abstractions;
using Enterprise.Observability.Tracing.Tracing;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Observability.Tracing.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddObservabilityTracing(this IServiceCollection services)
    {
        services.AddSingleton<IObservabilityTracer, ObservabilityTracer>();
        return services;
    }
}