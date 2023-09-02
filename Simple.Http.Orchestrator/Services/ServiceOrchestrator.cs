using Simple.Http.Orchestrator.Contracts;

namespace Simple.Http.Orchestrator.Services;

public class ServiceOrchestrator : IServiceOrchestrator
{
    private readonly IHttpClientFactory _httpClientFactory;
    public Dictionary<string, Request> _requestsState = new();

    public ServiceOrchestrator(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task ExecuteAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        _requestsState = activity.Requests.ToDictionary(x => x.Id, x => x);

        await Parallel.ForEachAsync
            (
                _requestsState.Values,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                },
                async (request, cancellationToken) =>
                {
                    await request.ExecuteAsync(_httpClientFactory, _requestsState, cancellationToken);
                }
            );
    }

    
}
