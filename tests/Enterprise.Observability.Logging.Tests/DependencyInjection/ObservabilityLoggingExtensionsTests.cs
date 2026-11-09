using Enterprise.Observability.Core.Abstractions;
using Enterprise.Observability.Logging.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Observability.Logging.Tests.DependencyInjection
{
    public sealed class ObservabilityLoggingExtensionsTests
    {
        [Fact]
        public void AddObservabilityLogging_Should_Register_ObservabilityLogger()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddObservabilityLogging();

            // Assert
            var descriptor = services.Single(service => service.ServiceType == typeof(IObservabilityLogger));

            Assert.Equal(typeof(ObservabilityLogger), descriptor.ImplementationType);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }
    }
}
