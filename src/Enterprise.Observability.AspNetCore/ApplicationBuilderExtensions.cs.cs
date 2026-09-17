using Enterprise.Observability.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Enterprise.Observability.AspNetCore;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseObservability(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ObservabilityMiddleware>();
    }
}