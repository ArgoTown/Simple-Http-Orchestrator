using Simple.Http.Orchestrator.Contracts;

namespace Simple.Http.Orchestrator.Services;

public class ServiceOrchestrator : IServiceOrchestrator
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ServiceOrchestrator(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task ExecuteByOrderAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        var requestsDictionary = activity.Requests
            .OrderBy(request => request.ExecutionOrder)
            .ToDictionary(x => x.Id);

        foreach (var (requestUniqueId, request) in requestsDictionary)
        {
            if (!request.Completed)
            {
                var dependencies = request.Dependencies.ToList();
                if (dependencies.Any())
                {
                    var isMultipleDependenciesCompleted = true;

                    foreach (var dependency in dependencies)
                    {
                        if (requestsDictionary[dependency].IsAllDependenciesCompleted)
                        {
                            continue;
                        }

                        isMultipleDependenciesCompleted = false;
                    }

                    if (!isMultipleDependenciesCompleted)
                    {
                        continue;
                    }

                    await SetRequestAsCompleted(request, cancellationToken);
                }
                else
                {
                    await SetRequestAsCompleted(request, cancellationToken);
                }
            }
        }
    }

    private async Task SetRequestAsCompleted(Requests request, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, request.Host.AbsoluteUri);
        var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            // Act on this request
        }

        request.IsAllDependenciesCompleted = true;
        request.Completed = true;
    }
}
