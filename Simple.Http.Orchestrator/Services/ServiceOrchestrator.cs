using Simple.Http.Orchestrator.Contracts;
using System.Text.Json;

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
        var httpRequest = new HttpRequestMessage(GetMethod(request.CallType.ToString()), request.Host.AbsoluteUri);

        foreach(var parameter in request.Parameters)
        {
            FillParameter(httpRequest, parameter);
        }

        var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            // Act on this request
        }

        request.IsAllDependenciesCompleted = true;
        request.Completed = true;
    }

    private static HttpMethod GetMethod(string method) 
    {
        if(method.Equals(nameof(HttpMethod.Post), StringComparison.OrdinalIgnoreCase))
        {
            return HttpMethod.Post;
        }

        if (method.Equals(nameof(HttpMethod.Get), StringComparison.OrdinalIgnoreCase))
        {
            return HttpMethod.Get;
        }

        if (method.Equals(nameof(HttpMethod.Delete), StringComparison.OrdinalIgnoreCase))
        {
            return HttpMethod.Delete;
        }

        if (method.Equals(nameof(HttpMethod.Patch), StringComparison.OrdinalIgnoreCase))
        {
            return HttpMethod.Patch;
        }

        if (method.Equals(nameof(HttpMethod.Put), StringComparison.OrdinalIgnoreCase))
        {
            return HttpMethod.Put;
        }

        if (method.Equals(nameof(HttpMethod.Head), StringComparison.OrdinalIgnoreCase))
        {
            return HttpMethod.Head;
        }

        if (method.Equals(nameof(HttpMethod.Trace), StringComparison.OrdinalIgnoreCase))
        {
            return HttpMethod.Trace;
        }

        if (method.Equals(nameof(HttpMethod.Options), StringComparison.OrdinalIgnoreCase))
        {
            return HttpMethod.Options;
        }

        throw new ArgumentOutOfRangeException(nameof(method));
    }

    private static void FillParameter(HttpRequestMessage httpRequestMessage, Parameters parameters)
    {
        if(parameters.Place.Equals(ParameterPlace.BODY))
        {
            httpRequestMessage.Content = new StringContent(parameters.Payload); // Todo mappers in future
            return;
        }

        if (parameters.Place.Equals(ParameterPlace.ROUTE))
        {
            var delimiter = parameters.Payload[0].Equals("/") ? string.Empty : "/";
            var payload = delimiter + parameters.Payload;
            httpRequestMessage.RequestUri = new Uri(httpRequestMessage.RequestUri!.AbsoluteUri + payload);
            return;
        }

        if (parameters.Place.Equals(ParameterPlace.HEADER))
        {
            var payloadDictionary = JsonSerializer.Deserialize<IDictionary<string, string>>(parameters.Payload);
            var payloadSchemaMapperDictionary = JsonSerializer.Deserialize<IDictionary<string, string>>(parameters.PayloadSchemaMapper);
            var headerValue = payloadDictionary!.SingleOrDefault(); // TODO handling error ?

            httpRequestMessage.Headers.TryAddWithoutValidation(headerValue.Key, headerValue.Value);
            return;
        }

        if (parameters.Place.Equals(ParameterPlace.QUERY))
        {
            var delimiter = parameters.Payload[0].Equals("/") ? string.Empty : "/";
            var payload = delimiter + parameters.Payload;
            httpRequestMessage.RequestUri = new Uri(httpRequestMessage.RequestUri!.AbsoluteUri + payload);
            return;
        }
    }
}
