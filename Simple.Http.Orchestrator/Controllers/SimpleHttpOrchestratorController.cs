using Json.Patch;
using Json.Path;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Simple.Http.Orchestrator.Contracts;
using Simple.Http.Orchestrator.Services;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Orchestrator.Controllers
{
    [ApiController]
    [Route("v1/bank")]
    public class SimpleHttpOrchestratorController : ControllerBase
    {
        private readonly ILogger<SimpleHttpOrchestratorController> _logger;
        private readonly IServiceOrchestrator _orchestrator;
        private readonly IConfiguration _configuration;

        public SimpleHttpOrchestratorController(
            ILogger<SimpleHttpOrchestratorController> logger,
            IServiceOrchestrator serviceOrchestrator,
            IConfiguration configuration)
        {
            _logger = logger;
            _orchestrator = serviceOrchestrator;
            _configuration = configuration;
        }

        [HttpPost("start")]
        public async Task<IActionResult> Start()
        {
            var activity = new Activity();
            _configuration.GetSection("Payload").Bind(activity);
            activity.Validate();


            var requestJson = "{ \"id\": \"\", \"documents\": [ \"11\", \"22\" ] }";
            //var patch = JsonSerializer.Deserialize<JsonPatch>(requestJson);
            var requestNodes = JsonNode.Parse(requestJson);
            //var pathForRequestToReplace = JsonPath.Parse("$.documents[*]");
            //var requestNodesResult = pathForRequestToReplace.Evaluate(requestNodes);

            var responseJson = "{ \"id\": \"\", \"documents\": [ \"DocOne\", \"DocTwo\" ] }";
            var responseNodes = JsonNode.Parse(responseJson);
            var pathForRequestToReplace = JsonPath.Parse("$.documents[*]");
            var requestNodesResult = pathForRequestToReplace.Evaluate(responseNodes);

            var patch = requestNodes.CreatePatch(requestNodesResult.Matches!);

            var patchResult = patch!.Apply(requestJson);

            var resultsss = JsonSerializer.Serialize(patchResult);


            await _orchestrator.ExecuteByOrderAsync(activity, CancellationToken.None);
            return Ok();
        }

        [HttpPost("payment")]
        public async Task<IActionResult> Payment()
        {
            _logger.LogInformation("Payment endpoint is called.");
            await Task.CompletedTask;
            return Ok();
        }

        [HttpPost("create-customer")]
        public async Task<IActionResult> CreateCustomer()
        {
            _logger.LogInformation("Create customer endpoint is called.");
            await Task.CompletedTask;
            return Ok();
        }

        [HttpPost("create-account")]
        public async Task<IActionResult> CreateAccount()
        {
            _logger.LogInformation("Create account endpoint is called.");
            await Task.CompletedTask;
            return Ok();
        }
    }
}