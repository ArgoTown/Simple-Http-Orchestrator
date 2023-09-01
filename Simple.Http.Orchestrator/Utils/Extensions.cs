using Json.More;
using Json.Patch;
using Json.Path;
using Json.Pointer;
using Simple.Http.Orchestrator.Contracts;
using Simple.Http.Orchestrator.Enums;
using System.Text.Json.Nodes;

namespace Simple.Http.Orchestrator.Utils;

public static class Extensions
{
    public static HttpMethod GetHttpMethod(this CallType callType)
    {
        var method = callType.ToString();
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

    public static string RemoveCurlyBrackets(this string value) => value.Replace("{", string.Empty).Replace("}", string.Empty);

    public static void FillParameter(this Parameter parameter, Dictionary<string, Request> state, HttpRequestMessage httpRequestMessage)
    {
        if (parameter.Place.Equals(ParameterPlace.BODY))
        {
            httpRequestMessage.Content = new StringContent(parameter.RequestPayload); // Todo mappers in future
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

            var resourceUrl = string.Join("/", resourceList);
            httpRequestMessage.RequestUri = new Uri(httpRequestMessage.RequestUri!.AbsoluteUri + resourceUrl);

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
            var requestText = "{ \"complex\": { \"Test\": \"Value\" }, \"documents\": { \"\": \"\",  \"pages\": [ \"11\", \"22\" ] } }";
            var responseText = "{ \"id\": \"\", \"documents\": [ \"DocOne\", \"DocTwo\" ] }";

            var requestJson = JsonNode.Parse(requestText);
            var responseJson = JsonNode.Parse(responseText);

            var requestPathAsJsonPointer = JsonPath.Parse("$.documents.pages[1]").AsJsonPointer();
            var responsePathAsJsonPointer = JsonPath.Parse("$.documents[1]").AsJsonPointer();

            var destination = JsonPointer.Parse(requestPathAsJsonPointer);
            var source = JsonPointer.Parse(responsePathAsJsonPointer);

            if (!source.TryEvaluate(responseJson, out var data) && data is null)
            {
                return;
            }

            var patch = new JsonPatch(PatchOperation.Replace(destination, data));

            var patched = patch.Apply(requestJson);

            Console.WriteLine(patched.Result.AsJsonString());
            /*var delimiter = parameters.Payload[0].Equals("/") ? string.Empty : "/";
            var payload = delimiter + parameters.Payload;
            httpRequestMessage.RequestUri = new Uri(httpRequestMessage.RequestUri!.AbsoluteUri + payload);
            */
            return;
        }
    }
}
