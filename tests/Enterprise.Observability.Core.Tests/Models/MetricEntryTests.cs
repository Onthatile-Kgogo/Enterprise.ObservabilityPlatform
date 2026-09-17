using Enterprise.Observability.Core.Models;

namespace Enterprise.Observability.Core.Tests.Models
{
    public sealed class MetricEntryTests
    {
        [Fact]
        public void Should_Create_MetricEntry_With_Expected_Values()
        {
            // Arrange
            var timestamp = DateTimeOffset.UtcNow;

            var context = new ObservabilityContext
            {
                CorrelationId = "correlation-123",
                TraceId = "trace-123",
                SpanId = "span-123"
            };

            var tags = new Dictionary<string, string>
            {
                ["Service"] = "OrderService",
                ["Environment"] = "Test"
            };

            // Act
            var metric = new MetricEntry
            {
                Timestamp = timestamp,
                Name = "orders.created",
                Value = 10,
                Context = context,
                Tags = tags
            };

            // Assert
            Assert.Equal(timestamp, metric.Timestamp);
            Assert.Equal("orders.created", metric.Name);
            Assert.Equal(10, metric.Value);
            Assert.Equal(context, metric.Context);
            Assert.Equal(tags, metric.Tags);
        }

        [Fact]
        public void Should_Initialize_Tags_When_Not_Provided()
        {
            // Act
            var metric = new MetricEntry
            {
                Timestamp = DateTimeOffset.UtcNow,
                Name = "orders.created",
                Value = 1
            };

            // Assert
            Assert.NotNull(metric.Tags);
            Assert.Empty(metric.Tags);
        }
    }
}
