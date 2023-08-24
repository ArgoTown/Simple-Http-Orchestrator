namespace Simple.Http.Orchestrator.Contracts;

public class Request
{
    public string Id { get; init; } = null!;
    public int ExecutionOrder { get; init; }
    public Uri Uri { get; init; } = null!;
    public bool Completed { get; set; }
    public bool IsAllDependenciesCompleted { get; set; }
    public IEnumerable<string> Dependencies { get; init; } = Enumerable.Empty<string>();
}
