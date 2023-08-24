using Simple.Http.Orchestrator.Contracts;

namespace Simple.Http.Orchestrator.Services;

public interface IServiceOrchestrator
{
    Task ExecuteByOrderAsync(Activity activity, CancellationToken cancellationToken = default);
}
