using Simple.Http.Orchestrator.Contracts;

namespace Simple.Http.Orchestrator.Services;

public interface IServiceOrchestrator
{
    void ExecuteAsync(Activity activity, CancellationToken cancellationToken = default);
}
