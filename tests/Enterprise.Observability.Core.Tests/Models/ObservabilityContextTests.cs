using Enterprise.Observability.Core.Models;

namespace Enterprise.Observability.Core.Tests.Models
{
    public sealed class ObservabilityContextTests
    {
        [Fact]
        public void Should_Create_Context_With_Expected_Values()
        {
            // Arrange
            var context = new ObservabilityContext
            {
                CorrelationId = "correlation-123",
                TraceId = "trace-123",
                SpanId = "span-123",
                ServiceName = "TestService",
                Environment = "Test"
            };

            // Assert
            Assert.Equal("correlation-123", context.CorrelationId);
            Assert.Equal("trace-123", context.TraceId);
            Assert.Equal("span-123", context.SpanId);
            Assert.Equal("TestService", context.ServiceName);
            Assert.Equal("Test", context.Environment);
        }

        [Fact]
        public void Should_Create_Context_With_Optional_Values_Null()
        {
            // Arrange
            var context = new ObservabilityContext();

            // Assert
            Assert.Null(context.CorrelationId);
            Assert.Null(context.TraceId);
            Assert.Null(context.SpanId);
            Assert.Null(context.ServiceName);
            Assert.Null(context.Environment);
        }
    }
}
