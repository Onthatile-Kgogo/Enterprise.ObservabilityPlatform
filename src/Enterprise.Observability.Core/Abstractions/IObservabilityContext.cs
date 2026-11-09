using Enterprise.Observability.Core.Models;

namespace Enterprise.Observability.Core.Abstractions
{
    public interface IObservabilityContext
    {
        ObservabilityContext Current { get; }
    }
}
