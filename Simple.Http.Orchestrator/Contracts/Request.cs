using System;

namespace Simple.Http.Orchestrator.Contracts;

public class Requests
{
    public string Id { get; init; } = null!;
    public int ExecutionOrder { get; init; }
    public Uri Host { get; init; } = null!;
    public CallType CallType { get; init; }
    public bool Completed { get; set; }
    public bool IsAllDependenciesCompleted { get; set; }
    public List<string> Dependencies { get; init; } = new List<string>();
    public List<Parameters> Parameters { get; init; } = new List<Parameters>();

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new ArgumentException($"Property {nameof(Id)} in JSON object {nameof(Requests)} not defined.");
        }

        if (Host is null || !Host.IsAbsoluteUri)
        {
            throw new ArgumentException($"Property {nameof(Host)} in JSON object {nameof(Requests)} not defined.");
        }

        if (CallType == CallType.UNDEFINED)
        {
            throw new ArgumentException($"Property {nameof(CallType)} in JSON object {nameof(Requests)} not defined.");
        }

        if (Dependencies is null)
        {
            throw new ArgumentException($"Property {nameof(Dependencies)} in JSON object {nameof(Requests)} not defined.");
        }

        if (!Parameters.Any())
        {
            throw new ArgumentException($"Property {nameof(Parameters)} in JSON object {nameof(Requests)} not defined.");
        }

        foreach (var parameter in Parameters)
        {
            parameter.Validate();
        }
    }
}

public class Parameters
{
    public ParameterPlace Place { get; init; }
    public string Payload { get; init; } = null!;
    public string PayloadSchemaMapper { get; init; } = null!;

    public void Validate()
    {
        if (Place == ParameterPlace.UNDEFINED)
        {
            throw new ArgumentException($"Property {nameof(Place)} in JSON object {nameof(Parameters)} not defined.");
        }

        if (string.IsNullOrWhiteSpace(Payload))
        {
            throw new ArgumentException($"Property {nameof(Payload)} in JSON object {nameof(Parameters)} not defined.");
        }

        if (string.IsNullOrWhiteSpace(PayloadSchemaMapper))
        {
            throw new ArgumentException($"Property {nameof(PayloadSchemaMapper)} in JSON object {nameof(Parameters)} not defined.");
        }
    }
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
