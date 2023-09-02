using Simple.Http.Orchestrator.Contracts;

namespace Simple.Http.Orchestrator.Services;

public interface IServiceOrchestrator
{
    Task ExecuteAsync(Activity activity, CancellationToken cancellationToken = default);
}
