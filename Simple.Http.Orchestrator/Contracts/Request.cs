using System;

namespace Simple.Http.Orchestrator.Contracts;

public class Request
{
    public string Id { get; init; } = null!;
    public int ExecutionOrder { get; init; }
    public Uri Host { get; init; } = null!;
    public CallType CallType { get; init; }
    public bool IsCompleted { get; private set; }
    public bool IsFailed { get; private set; }
    public List<Parameter> Parameters { get; init; } = new List<Parameter>();

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new ArgumentException($"Property {nameof(Id)} in JSON object {nameof(Request)} not defined.");
        }

        if (Host is null || !Host.IsAbsoluteUri)
        {
            throw new ArgumentException($"Property {nameof(Host)} in JSON object {nameof(Request)} not defined.");
        }

        if (CallType == CallType.UNDEFINED)
        {
            throw new ArgumentException($"Property {nameof(CallType)} in JSON object {nameof(Request)} not defined.");
        }

        if (!Parameters.Any())
        {
            throw new ArgumentException($"Property {nameof(Parameters)} in JSON object {nameof(Request)} not defined.");
        }

        foreach (var parameter in Parameters)
        {
            parameter.Validate();
        }
    }
}

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
public class DependencyMap
{
    public string Name { get; init; } = null!;
    public List<Map> ResponseToRequestMaps { get; init; } = new();
}


public class Map
{
    public string From { get; init; } = null!;
    public string To { get; init; } = null!;
}

    public enum CallType
{
    UNDEFINED,
    POST, 
    PUT, 
    DELETE, 
    GET, 
    HEAD
}

public enum ParameterPlace
{
    UNDEFINED,
    ROUTE,
    QUERY,
    BODY,
    HEADER
}
