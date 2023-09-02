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
        public async Task<ActionResult> StartAsync()
        {
            var activity = new Activity();
            _configuration.GetSection("Payload").Bind(activity);
            var errors = activity.Validate(); 
            if(errors.Any())
            {
                return BadRequest(errors);
            }

            await _orchestrator.ExecuteAsync(activity, CancellationToken.None);
            return Ok();
        }

        [HttpPost("payment")]
        public async Task<IActionResult> Payment()
        {
            _logger.LogInformation("Payment endpoint is called.");
            await Task.CompletedTask;
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login()
        {
            _logger.LogInformation("Login endpoint is called.");
            await Task.CompletedTask;
            return Ok(new { customer = new { address = new { id = "CCE9C5A0-84F3-4433-88EB-FF908A465F7A" } } });
        }

        [HttpPost("create-customer/{id}/address/{addressId}/bind")]
        public async Task<IActionResult> CreateCustomer([FromQuery] Guid traceId, [FromQuery] Guid addressId)
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