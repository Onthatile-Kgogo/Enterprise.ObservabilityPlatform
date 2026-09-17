using Enterprise.Observability.Core.Enums;
using Enterprise.Observability.Core.Models;

namespace Enterprise.Observability.Core.Abstractions
{
    public interface IObservabilityLogger
    {
        void Log(ObservabilityLogLevel level, LogEntry entry);
    }
}
