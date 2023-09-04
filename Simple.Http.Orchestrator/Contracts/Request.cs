using Json.More;
using Json.Patch;
using Json.Path;
using Json.Pointer;
using Simple.Http.Orchestrator.Enums;
using Simple.Http.Orchestrator.Utils;
using System.Net.Mime;
using System.Text.Json.Nodes;

namespace Simple.Http.Orchestrator.Contracts;

public class Request
{
    public string Id { get; init; } = null!;
    public Uri Host { get; init; } = null!;
    public CallType CallType { get; init; }
    public bool IsCompleted { get; private set; }
    public bool IsFailed { get; private set; }
    public string Response { get; private set; } = string.Empty;
    public List<Parameter> Parameters { get; init; } = new();

    private readonly SemaphoreSlim _semaphoreSlim = new(1);
    private HttpMethod HttpMethod => CallType.GetHttpMethod();
    private string _query = string.Empty;
    private string _route = string.Empty;

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

    public async Task ExecuteAsync(
        ILogger logger,
        IHttpClientFactory httpClientFactory, 
        Dictionary<string, Request> state, 
        CancellationToken cancellationToken = default)
    {
        if (IsCompleted || IsFailed)
        {
            logger.LogInformation($"Retrieving already received response in service {Id} on thread {Environment.CurrentManagedThreadId}");

            return;
        }

        var httpRequest = new HttpRequestMessage(HttpMethod, Host.AbsoluteUri);

        var tasks = new List<Task>();

        foreach(var parameter in Parameters)
        {

            tasks.Add(FillParameter(
                logger,
            parameter,
            state,
            httpRequest,
            httpClientFactory,
            cancellationToken));
        }

        await Task.WhenAll(tasks);

        logger.LogInformation($"Entering semaphore for service {Id} on thread {Environment.CurrentManagedThreadId}. Time {DateTime.UtcNow.ToString("O")}");

        await _semaphoreSlim.WaitAsync(200, cancellationToken);

        if (IsCompleted || IsFailed)
        {
            logger.LogInformation($"Retrieving already received response in service {Id} on thread {Environment.CurrentManagedThreadId}");

            return;
        }

        httpRequest.RequestUri = new Uri(httpRequest.RequestUri!.AbsoluteUri + _route + _query);

        using var httpClient = httpClientFactory.CreateClient();
        var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            IsFailed = true;

            _semaphoreSlim.Release();
            // Act on this request
            return;
        }

        Response = await response.Content.ReadAsStringAsync(cancellationToken);

        IsCompleted = true;

        _semaphoreSlim.Release();
    }

    public async Task FillParameter(
        ILogger logger,
        Parameter parameter, 
        Dictionary<string, Request> state, 
        HttpRequestMessage httpRequestMessage,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken = default)
    {
        if (parameter.Place.Equals(ParameterPlace.BODY))
        {
            var requestPayload = parameter.RequestPayload;
            if (parameter.RequestPayloadSchemaMaps.Any())
            {
                JsonNode? requestJson;
                try
                {
                    requestJson = JsonNode.Parse(requestPayload)
                    ?? throw new ArgumentNullException($"Tried to parse this payload {parameter.RequestPayload} but value returned null.");
                }
                catch (Exception)
                {
                    throw new InvalidOperationException($"Parsing failed of this payload {parameter.RequestPayload} in request with name {Id}.");
                }

                foreach (var map in parameter.RequestPayloadSchemaMaps)
                {
                    var requestPathAsJsonPointer = JsonPath.Parse(map.Destination).AsJsonPointer();
                    var destination = JsonPointer.Parse(requestPathAsJsonPointer);

                    JsonNode? node;

                    if (map.Source.StartsWith("{"))
                    {
                        try
                        {
                            var parsed = JsonNode.Parse(map.Source);

                            if (!destination.TryEvaluate(parsed, out var data))
                            {
                                // Throw ?
                            }

                            node = data;
                        }
                        catch (Exception)
                        {
                            throw new InvalidOperationException($"Parsing failed of this payload source {map.Source} in request with name {Id}.");
                        }
                    }
                    else
                    {
                        node = map.Source.ToJsonDocument().RootElement.AsNode();                       
                    }

                    var patch = new JsonPatch(PatchOperation.Replace(destination, node));

                    requestJson = patch.Apply(requestJson).Result;
                }

                requestPayload = requestJson.ToJsonString();
            }

            if(parameter.Dependencies.Any(dependency => dependency.ResponseToRequestMaps.Any()))
            {
                JsonNode? requestJson;
                try
                {
                    requestJson = JsonNode.Parse(requestPayload)
                    ?? throw new ArgumentNullException($"Tried to parse this payload {parameter.RequestPayload} but value returned null.");
                }
                catch (Exception)
                {
                    throw new InvalidOperationException($"Parsing failed of this payload {parameter.RequestPayload} in request with name {Id}.");
                }

                foreach (var dependency in parameter.Dependencies)
                {
                    logger.LogInformation($"Retrieve request object with name {dependency.Name} in parameter {parameter.Place}. Thread id {Environment.CurrentManagedThreadId}. Time {DateTime.UtcNow.ToString("O")}");
                    var request = state[dependency.Name];
                    await request.ExecuteAsync(logger, httpClientFactory, state, cancellationToken);

                    var responseJsonNodes = JsonNode.Parse(request.Response);

                    foreach (var map in dependency.ResponseToRequestMaps)
                    {
                        var dependencyResponsePath = JsonPath.Parse(map.Source).AsJsonPointer();
                        var payloadRequestPath = JsonPath.Parse(map.Destination).AsJsonPointer();

                        var dependencyResponsePathPointer = JsonPointer.Parse(dependencyResponsePath);
                        var payloadRequestPathPointer = JsonPointer.Parse(payloadRequestPath);

                        if (!dependencyResponsePathPointer.TryEvaluate(responseJsonNodes, out var responseData) && responseData is null)
                        {
                            throw new ArgumentException("Either source path or destination path is wrong.");
                        }

                        var patch = new JsonPatch(PatchOperation.Replace(payloadRequestPathPointer, responseData));

                        var patchedResult = patch.Apply(requestJson).Result;
                    }

                    requestPayload = requestJson.ToJsonString();
                }
            }

            httpRequestMessage.Content = new StringContent(
                requestPayload, 
                System.Text.Encoding.UTF8, 
                MediaTypeNames.Application.Json); // Todo mappers in future

            return;
        }

        if (parameter.Place.Equals(ParameterPlace.ROUTE))
        {
            var resourceList = parameter.RequestPayload.Split("/").Select(item => item.RemoveCurlyBrackets()).ToList();
            foreach (var map in parameter.RequestPayloadSchemaMaps)
            {
                var resourceIndexToUpdate = resourceList.IndexOf(map.Destination.RemoveCurlyBrackets());
                resourceList[resourceIndexToUpdate] = map.Source;
            }

            _route = $"/{string.Join("/", resourceList)}";
            return;
        }

        if (parameter.Place.Equals(ParameterPlace.HEADER))
        {
            foreach (var map in parameter.RequestPayloadSchemaMaps)
            {
                httpRequestMessage.Headers.TryAddWithoutValidation(map.Destination, map.Source);
            }

            return;
        }

        if (parameter.Place.Equals(ParameterPlace.QUERY))
        {
            var parsedRequestQuery = parameter.RequestPayload;

            if (parameter.RequestPayloadSchemaMaps.Any())
            {
                foreach(var map in parameter.RequestPayloadSchemaMaps)
                {
                    parsedRequestQuery = parsedRequestQuery.Replace(map.Destination, map.Source);
                }
            }

            if (parameter.Dependencies.Any(dependency => dependency.ResponseToRequestMaps.Any()))
            {
                foreach (var dependency in parameter.Dependencies)
                {
                    logger.LogInformation($"Retrieve request object with name {dependency.Name} in parameter {parameter.Place}. Thread id {Environment.CurrentManagedThreadId}. Time {DateTime.UtcNow.ToString("O")}");

                    var request = state[dependency.Name];
                    await request.ExecuteAsync(logger, httpClientFactory, state, cancellationToken);

                    var responseJsonNodes = JsonNode.Parse(request.Response);

                    foreach(var map in dependency.ResponseToRequestMaps)
                    {
                        var responseJsonPathForDestination = JsonPath.Parse(map.Source).AsJsonPointer();
                        var responseJsonPointerForDestination = JsonPointer.Parse(responseJsonPathForDestination);
                        if (!responseJsonPointerForDestination.TryEvaluate(responseJsonNodes, out var responseData) && responseData is null)
                        {
                            throw new ArgumentException("Either source path or destination path is wrong.");
                        }

                        var actualValue = responseData!.AsValue().ToString();

                        parsedRequestQuery = parsedRequestQuery.Replace(map.Destination, actualValue);
                    }
                }
            }

            _query = $"?{parsedRequestQuery}";
            return;
        }
    }
}


