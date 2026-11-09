using Enterprise.Observability.Core.Models;

namespace Enterprise.Observability.Core.Tests.Models
{
    public sealed class LogEntryTests
    {
        [Fact]
        public void Should_Create_LogEntry_With_Expected_Values()
        {
            // Arrange
            var timestamp = DateTimeOffset.UtcNow;

            var context = new ObservabilityContext
            {
                CorrelationId = "correlation-123",
                TraceId = "trace-123",
                SpanId = "span-123"
            };

            var properties = new Dictionary<string, object?>
            {
                ["OrderId"] = 12345,
                ["Operation"] = "CreateOrder"
            };

            // Act
            var entry = new LogEntry
            {
                Timestamp = timestamp,
                Message = "Order created",
                Exception = null,
                Source = "OrderService",
                Context = context,
                Properties = properties
            };

            // Assert
            Assert.Equal(timestamp, entry.Timestamp);
            Assert.Equal("Order created", entry.Message);
            Assert.Null(entry.Exception);
            Assert.Equal("OrderService", entry.Source);
            Assert.Equal(context, entry.Context);
            Assert.Equal(properties, entry.Properties);
        }

        [Fact]
        public void Should_Create_LogEntry_With_Exception()
        {
            // Arrange
            var exception = new InvalidOperationException("Something went wrong");

            // Act
            var entry = new LogEntry
            {
                Timestamp = DateTimeOffset.UtcNow,
                Message = "Operation failed",
                Exception = exception
            };

            // Assert
            Assert.Same(exception, entry.Exception);
        }

        [Fact]
        public void Should_Initialize_Properties_When_Not_Provided()
        {
            // Act
            var entry = new LogEntry
            {
                Timestamp = DateTimeOffset.UtcNow,
                Message = "Test"
            };

            // Assert
            Assert.NotNull(entry.Properties);
            Assert.Empty(entry.Properties);
        }
    }
}
