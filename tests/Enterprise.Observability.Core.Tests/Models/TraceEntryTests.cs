using Enterprise.Observability.Core.Models;

namespace Enterprise.Observability.Core.Tests.Models
{
    public sealed class TraceEntryTests
    {
        [Fact]
        public void Should_Create_TraceEntry_With_Expected_Values()
        {
            // Arrange
            var timestamp = DateTimeOffset.UtcNow;

            var context = new ObservabilityContext
            {
                CorrelationId = "correlation-123",
                TraceId = "trace-123",
                SpanId = "span-123"
            };

            var attributes = new Dictionary<string, object?>
            {
                ["Operation"] = "CreateOrder",
                ["OrderId"] = 12345
            };

            // Act
            var trace = new TraceEntry
            {
                Timestamp = timestamp,
                Name = "orders.create",
                TraceId = "trace-123",
                SpanId = "span-456",
                ParentSpanId = "span-123",
                Context = context,
                Attributes = attributes
            };

            // Assert
            Assert.Equal(timestamp, trace.Timestamp);
            Assert.Equal("orders.create", trace.Name);
            Assert.Equal("trace-123", trace.TraceId);
            Assert.Equal("span-456", trace.SpanId);
            Assert.Equal("span-123", trace.ParentSpanId);
            Assert.Equal(context, trace.Context);
            Assert.Equal(attributes, trace.Attributes);
        }

        [Fact]
        public void Should_Create_TraceEntry_Without_ParentSpan()
        {
            // Act
            var trace = new TraceEntry
            {
                Timestamp = DateTimeOffset.UtcNow,
                Name = "orders.create",
                TraceId = "trace-123",
                SpanId = "span-456"
            };

            // Assert
            Assert.Null(trace.ParentSpanId);
        }

        [Fact]
        public void Should_Initialize_Attributes_When_Not_Provided()
        {
            // Act
            var trace = new TraceEntry
            {
                Timestamp = DateTimeOffset.UtcNow,
                Name = "orders.create",
                TraceId = "trace-123",
                SpanId = "span-456"
            };

            // Assert
            Assert.NotNull(trace.Attributes);
            Assert.Empty(trace.Attributes);
        }
    }
}
