using Enterprise.Observability.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Observability.Logging.DependencyInjection
{
    public static class ObservabilityLoggingExtensions
    {
        public static IServiceCollection AddObservabilityLogging(this IServiceCollection services)
        {
            services.AddSingleton<IObservabilityLogger, ObservabilityLogger>();
            return services;
        }
    }
}
