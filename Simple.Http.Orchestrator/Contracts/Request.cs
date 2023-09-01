using Simple.Http.Orchestrator.Enums;
using Simple.Http.Orchestrator.Utils;

namespace Simple.Http.Orchestrator.Contracts;

public class Request
{
    public string Id { get; init; } = null!;
    public int ExecutionOrder { get; init; }
    public Uri Host { get; init; } = null!;
    public CallType CallType { get; init; }
    public HttpMethod HttpMethod => CallType.GetHttpMethod();
    public bool IsCompleted { get; private set; }
    public bool IsFailed { get; private set; }
    public List<Parameter> Parameters { get; init; } = new List<Parameter>();
    private readonly SemaphoreSlim _semaphoreSlim = new(1);

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
        }

        IsCompleted = true;

        _semaphoreSlim.Release();
    }
}


