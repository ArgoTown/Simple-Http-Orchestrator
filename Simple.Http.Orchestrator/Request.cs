using Simple.Http.Orchestrator.Services;

namespace Simple.Http.Orchestrator;

public class Request
{
    public string Id { get; init; } = null!;
    public int ExecutionOrder { get; init; }
    public string ServiceCall { get; init; } = null!;
    public Uri Uri { get; init; } = null!;
    public bool Completed { get; set; }
    public bool IsAllDependenciesCompleted { get; set; }
    public IEnumerable<string> Dependencies { get; init; } = Enumerable.Empty<string>();
}
