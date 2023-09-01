using Microsoft.AspNetCore.Mvc;
using Simple.Http.Orchestrator.Contracts;
using Simple.Http.Orchestrator.Services;

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
        public ActionResult Start()
        {
            var activity = new Activity();
            _configuration.GetSection("Payload").Bind(activity);
            activity.Validate();            

            _orchestrator.ExecuteAsync(activity, CancellationToken.None);
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