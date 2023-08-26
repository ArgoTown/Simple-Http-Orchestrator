namespace Simple.Http.Orchestrator.Contracts;

public class Requests
{
    public string Id { get; init; } = null!;
    public int ExecutionOrder { get; init; }
    public Uri Host { get; init; } = null!;
    public bool Completed { get; set; }
    public bool IsAllDependenciesCompleted { get; set; }
    public List<string> Dependencies { get; init; } = new List<string>();
    public List<Versions> Versions { get; init; } = new List<Versions>();

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

        if (Dependencies is null)
        {
            throw new ArgumentException($"Property {nameof(Dependencies)} in JSON object {nameof(Requests)} not defined.");
        }

        foreach (var version in Versions)
        {
            version.Validate();
        }
    }
}

public class Versions
{
    public string ApiVersion { get; init; } = null!;
    public CallType CallType { get; init; }
    public string Resource { get; init; } = null!;
    public List<RequestParameters> RequestParameters { get; init; } = new List<RequestParameters>();

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiVersion))
        {
            throw new ArgumentException($"Property {nameof(CallType)} in JSON object {nameof(Versions)} not defined.");
        }

        if(CallType == CallType.UNDEFINED)
        {
            throw new ArgumentException($"Property {nameof(CallType)} in JSON object {nameof(Versions)} not defined.");
        }

        if (string.IsNullOrWhiteSpace(Resource))
        {
            throw new ArgumentException($"Property {nameof(Resource)} in JSON object {nameof(Versions)} not defined.");
        }

        if (!RequestParameters.Any())
        {
            throw new ArgumentException($"Property {nameof(RequestParameters)} in JSON object {nameof(Versions)} not defined.");
        }

        foreach (var parameter in RequestParameters)
        {
            parameter.Validate();
        }
    }
}

public class RequestParameters
{
    public ParameterPlace Place { get; init; }
    public string Payload { get; init; } = null!;
    public string PayloadSchemaMapper { get; init; } = null!;

    public void Validate()
    {
        if (Place == ParameterPlace.UNDEFINED)
        {
            throw new ArgumentException($"Property {nameof(Place)} in JSON object {nameof(RequestParameters)} not defined.");
        }

        if (string.IsNullOrWhiteSpace(Payload))
        {
            throw new ArgumentException($"Property {nameof(Payload)} in JSON object {nameof(RequestParameters)} not defined.");
        }

        if (string.IsNullOrWhiteSpace(PayloadSchemaMapper))
        {
            throw new ArgumentException($"Property {nameof(PayloadSchemaMapper)} in JSON object {nameof(RequestParameters)} not defined.");
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
    HEADER, 
    FORM
}
