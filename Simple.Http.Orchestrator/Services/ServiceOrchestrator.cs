using Simple.Http.Orchestrator.Contracts;
using System.Text.Json;

namespace Simple.Http.Orchestrator.Services;

public class ServiceOrchestrator : IServiceOrchestrator
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Dictionary<string, Request> _requestsState = new();

    public ServiceOrchestrator(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task ExecuteByOrderAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        _requestsState = activity.Requests
            .OrderBy(request => request.ExecutionOrder)
            .ToDictionary(x => x.Id);

        foreach (var (requestUniqueId, request) in _requestsState)
        {
            if (!request.IsCompleted)
            {
                await SetRequestAsCompleted(request, cancellationToken);
            }
        }
    }

    private async Task SetRequestAsCompleted(Request request, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var httpRequest = new HttpRequestMessage(GetMethod(request.CallType.ToString()), request.Host.AbsoluteUri);

        foreach (var parameter in request.Parameters)
        {
            FillParameter(httpRequest, parameter);
        }

        var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            // Act on this request
        }
    }

    private static HttpMethod GetMethod(string method)
    {
        if (method.Equals(nameof(HttpMethod.Post), StringComparison.OrdinalIgnoreCase))
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

    private static void FillParameter(HttpRequestMessage httpRequestMessage, Parameter parameters)
    {
        if (parameters.Place.Equals(ParameterPlace.BODY))
        {
            httpRequestMessage.Content = new StringContent(parameters.RequestPayload); // Todo mappers in future
            return;
        }

        if (parameters.Place.Equals(ParameterPlace.ROUTE))
        {
            var resourceList = parameters.RequestPayload.Split("/").Select(item => item.Replace("{", string.Empty).Replace("}", string.Empty)).ToList();
            foreach (var map in parameters.ReuestPayloadSchemaMaps)
            {
                var mapToTrimmedValue = map.To.Replace("{", string.Empty).Replace("}", string.Empty);
                
                var resourceIndexToUpdate = resourceList.IndexOf(mapToTrimmedValue);
                resourceList[resourceIndexToUpdate] = map.From;
            }

            var resourceUrl = string.Join("/", resourceList);
            httpRequestMessage.RequestUri = new Uri(httpRequestMessage.RequestUri!.AbsoluteUri + resourceUrl);
            
            return;
        }

        if (parameters.Place.Equals(ParameterPlace.HEADER))
        {
            foreach (var header in _httpContextAccessor.HttpContext.Request.Headers.)
            { 
            }

            foreach (var map in parameters.ReuestPayloadSchemaMaps)
            {
                httpRequestMessage.Headers.TryAddWithoutValidation(map.To, map.From);
            }

                var payloadDictionary = JsonSerializer.Deserialize<IDictionary<string, string>>(parameters.RequestPayload);
            //var payloadSchemaMapperDictionary = JsonSerializer.Deserialize<IDictionary<string, string>>(parameters.PayloadSchemaMapper);
            var headerValue = payloadDictionary!.SingleOrDefault(); // TODO handling error ?

            httpRequestMessage.Headers.TryAddWithoutValidation(headerValue.Key, headerValue.Value);
            return;
        }

        if (parameters.Place.Equals(ParameterPlace.QUERY))
        {
            /*var delimiter = parameters.Payload[0].Equals("/") ? string.Empty : "/";
            var payload = delimiter + parameters.Payload;
            httpRequestMessage.RequestUri = new Uri(httpRequestMessage.RequestUri!.AbsoluteUri + payload);
            */
            return;
        }
    }
}
