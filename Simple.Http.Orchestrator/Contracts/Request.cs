using Simple.Http.Orchestrator.Enums;
using Simple.Http.Orchestrator.Utils;

namespace Simple.Http.Orchestrator.Contracts;

public class Request
{
    public string Id { get; init; } = null!;
    public int ExecutionOrder { get; init; }
    public Uri Host { get; init; } = null!;
    public CallType CallType { get; init; }
    public bool IsCompleted { get; private set; }
    public bool IsFailed { get; private set; }
    public string Response { get; private set; } = string.Empty;
    public List<Parameter> Parameters { get; init; } = new();

    private readonly SemaphoreSlim _semaphoreSlim = new(1);
    private HttpMethod HttpMethod => CallType.GetHttpMethod();

    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Id))
        {
            errors.Add($"Property {nameof(Id)} in JSON object {nameof(Request)} not defined.");
        }

        if (Host is null || !Host.IsAbsoluteUri)
        {
            errors.Add($"Property {nameof(Host)} in JSON object {nameof(Request)} not defined.");
        }

        if (CallType == CallType.UNDEFINED)
        {
            errors.Add($"Property {nameof(CallType)} in JSON object {nameof(Request)} not defined.");
        }

        if (!Parameters.Any())
        {
            errors.Add($"Property {nameof(Parameters)} in JSON object {nameof(Request)} not defined.");
        }

        Parameters.ForEach(parameter => errors.AddRange(parameter.Validate()));

        return errors;
    }

    public async Task ExecuteAsync(HttpClient httpClient, Dictionary<string, Request> state, CancellationToken cancellationToken = default)
    {
        if(IsCompleted || IsFailed)
        {
            return;
        }

        await _semaphoreSlim.WaitAsync(cancellationToken);

        if (IsCompleted || IsFailed)
        {
            return;
        }

        var httpRequest = new HttpRequestMessage(HttpMethod, Host.AbsoluteUri);

        Parameters.ForEach(parameter => parameter.FillParameter(state, httpRequest));

        var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            IsFailed = true;
            // Act on this request
            return;
        }

        Response = await response.Content.ReadAsStringAsync(cancellationToken);

        IsCompleted = true;

        _semaphoreSlim.Release();
    }
}


