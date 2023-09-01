namespace Simple.Http.Orchestrator.Contracts;

public class Parameter
{
    public ParameterPlace Place { get; init; }
    public string RequestPayload { get; init; } = null!;
    public List<Map> ReuestPayloadSchemaMaps { get; init; } = new();
    public List<DependencyMap> Dependencies { get; init; } = new();

    public void Validate()
    {
        if (Place == ParameterPlace.UNDEFINED)
        {
            throw new ArgumentException($"Property {nameof(Place)} in JSON object {nameof(Parameter)} not defined.");
        }
    }
}
