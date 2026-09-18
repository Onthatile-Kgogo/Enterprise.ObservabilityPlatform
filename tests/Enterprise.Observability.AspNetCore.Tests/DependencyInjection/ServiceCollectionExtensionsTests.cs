using Enterprise.Observability.AspNetCore.DependencyInjection;
using Enterprise.Observability.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Observability.AspNetCore.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddObservability_ShouldRegisterObservabilityTracer()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddObservability();
        using var provider = services.BuildServiceProvider();

        // Assert
        var tracer = provider.GetService<IObservabilityTracer>();
        var contextAccessor = provider.GetService<IObservabilityContext>();

        Assert.NotNull(tracer);
        Assert.NotNull(contextAccessor);
    }
}