using Simple.Http.Orchestrator.Contracts;

namespace Simple.Http.Orchestrator.Services;

public class ServiceOrchestrator : IServiceOrchestrator
{
    private readonly IHttpClientFactory _httpClientFactory;
    private Dictionary<string, Request> _requestsState = new();

    public ServiceOrchestrator(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public void ExecuteAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        _requestsState = activity.Requests
            .OrderBy(request => request.ExecutionOrder)
            .ToDictionary(x => x.Id, x => x);

        Parallel.ForEachAsync
            (
                _requestsState.Values,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount * 2
                },
                async (request, cancellationToken) =>
                {
                    await request.ExecuteAsync(_httpClientFactory, _requestsState, cancellationToken);
                }
            );
    }

    
}
