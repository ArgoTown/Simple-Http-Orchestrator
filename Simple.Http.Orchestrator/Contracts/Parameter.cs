using Simple.Http.Orchestrator.Enums;

namespace Simple.Http.Orchestrator.Contracts;

public class Parameter
{
    public ParameterPlace Place { get; init; }
    public string RequestPayload { get; init; } = null!;
    public List<Map> RequestPayloadSchemaMaps { get; init; } = new();
    public List<DependencyMap> Dependencies { get; init; } = new();

    public List<string> Validate()
    {
        var errors = new List<string>();

        if (Place == ParameterPlace.UNDEFINED)
        {
            errors.Add($"Property {nameof(Place)} in JSON object {nameof(Parameter)} not defined.");
        }

        return errors;
    }
}
