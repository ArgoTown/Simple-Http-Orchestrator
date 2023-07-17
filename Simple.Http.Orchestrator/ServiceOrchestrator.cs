using Simple.Http.Orchestrator.Services;

namespace Simple.Http.Orchestrator;

public class ServiceOrchestrator
{
    private readonly IDictionary<string, IServiceCall> _services = new Dictionary<string, IServiceCall>();

    public ServiceOrchestrator(IDictionary<string, IServiceCall> services)
    {
        if (services is null || services.Count.Equals(0))
        {
            throw new ArgumentNullException(nameof(services));
        }

        foreach (var service in services)
        {
            _services.Add(service.Key, service.Value);
        }
    }

    public async Task ExecuteByOrder(Activity activity, CancellationToken cancellationToken)
    {
        var requestsDictionary = activity.Requests
            .OrderBy(request => request.ExecutionOrder)
            .ToDictionary(x => x.Id);

        foreach (var (requestUniqueId, request) in requestsDictionary)
        {
            var service = _services[request.ServiceCall];
            if (service != null && (!request.Completed))
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

                    await SetRequestAsCompleted(request, service, cancellationToken);
                }
                else
                {
                    await SetRequestAsCompleted(request, service, cancellationToken);
                }
            }
        }
    }

    private static async Task SetRequestAsCompleted(Request request, IServiceCall service, CancellationToken cancellationToken)
    {
        await service.Execute(request.Uri, cancellationToken);
        request.IsAllDependenciesCompleted = true;
        request.Completed = true;
    }
}
