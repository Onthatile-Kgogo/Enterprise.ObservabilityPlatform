using Enterprise.Observability.Core.Abstractions;
using Enterprise.Observability.Core.Enums;
using Enterprise.Observability.Core.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace Enterprise.Observability.Logging.Tests;

public sealed class ObservabilityLoggerTests
{
    private readonly Mock<ILogger<ObservabilityLogger>> _loggerMock;
    private readonly IObservabilityLogger _logger;

    public ObservabilityLoggerTests()
    {
        _loggerMock = new Mock<ILogger<ObservabilityLogger>>();
        _logger = new ObservabilityLogger(_loggerMock.Object);
    }

    [Fact]
    public void Log_ShouldWriteLogMessage()
    {
        // Arrange
        var entry = new LogEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Message = "Test message",
            Source = "Test"
        };

        // Act
        _logger.Log(ObservabilityLogLevel.Information, entry);

        // Assert
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Log_Should_Write_Message_At_Information_Level()
    {
        // Arrange
        var entry = new LogEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Message = "Order created"
        };

        // Act
        _logger.Log(
            ObservabilityLogLevel.Information,
            entry);

        // Assert
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Log_ShouldThrow_WhenEntryIsNull()
    {
        // Act
        var action = () => _logger.Log(ObservabilityLogLevel.Information, null!);

        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }

    [Theory]
    [InlineData(ObservabilityLogLevel.Trace, LogLevel.Trace)]
    [InlineData(ObservabilityLogLevel.Debug, LogLevel.Debug)]
    [InlineData(ObservabilityLogLevel.Information, LogLevel.Information)]
    [InlineData(ObservabilityLogLevel.Warning, LogLevel.Warning)]
    [InlineData(ObservabilityLogLevel.Error, LogLevel.Error)]
    [InlineData(ObservabilityLogLevel.Critical, LogLevel.Critical)]
    public void Log_ShouldMapLogLevelCorrectly(ObservabilityLogLevel observabilityLevel, LogLevel expectedLevel)
    {
        // Arrange
        var entry = new LogEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Message = "Test message"
        };

        // Act
        _logger.Log(observabilityLevel, entry);

        // Assert
        _loggerMock.Verify(
            logger => logger.Log(
                expectedLevel,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Log_Should_Pass_Exception_To_Underlying_Logger()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        var entry = new LogEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Message = "Operation failed",
            Exception = exception
        };

        // Act
        _logger.Log(ObservabilityLogLevel.Error, entry);

        // Assert
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Log_Should_Include_Observability_Context()
    {
        // Arrange
        var context = new ObservabilityContext
        {
            CorrelationId = "correlation-123",
            TraceId = "trace-123",
            SpanId = "span-123"
        };

        var entry = new LogEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Message = "Order created",
            Context = context
        };

        // Act
        _logger.Log(ObservabilityLogLevel.Information, entry);

        // Assert
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(
                    (state, _) =>
                        Equals(((IReadOnlyDictionary<string, object?>)state)["CorrelationId"], "correlation-123") &&
                        Equals(((IReadOnlyDictionary<string, object?>)state)["TraceId"], "trace-123") &&
                        Equals(((IReadOnlyDictionary<string, object?>)state)["SpanId"], "span-123")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Log_Should_Include_Custom_Properties()
    {
        // Arrange
        var entry = new LogEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Message = "Order created",
            Properties = new Dictionary<string, object?>
            {
                ["OrderId"] = 12345,
                ["Operation"] = "CreateOrder"
            }
        };

        // Act
        _logger.Log(ObservabilityLogLevel.Information, entry);

        // Assert
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    Equals(((IReadOnlyDictionary<string, object?>)state)["OrderId"], 12345) &&
                    Equals(((IReadOnlyDictionary<string, object?>)state)["Operation"], "CreateOrder")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Log_Should_Include_Timestamp()
    {
        // Arrange
        var timestamp = new DateTimeOffset(
            2026,
            9,
            18,
            10,
            30,
            0,
            TimeSpan.Zero);

        var entry = new LogEntry
        {
            Timestamp = timestamp,
            Message = "Order created"
        };

        // Act
        _logger.Log(ObservabilityLogLevel.Information, entry);

        // Assert
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => Equals(((IReadOnlyDictionary<string, object?>)state)["Timestamp"], timestamp)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}