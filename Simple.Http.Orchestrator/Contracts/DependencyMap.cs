namespace Simple.Http.Orchestrator.Contracts;

public class DependencyMap
{
    public string Name { get; init; } = null!;
    public List<Map> ResponseToRequestMaps { get; init; } = new();
}
