using Microsoft.AspNetCore.Mvc;
using Simple.Http.Orchestrator.Contracts;
using Simple.Http.Orchestrator.Contracts.Requests;
using Simple.Http.Orchestrator.Contracts.Responses;
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
            if (errors.Any())
            {
                return BadRequest(errors);
            }

            await _orchestrator.ExecuteAsync(activity, CancellationToken.None);
            return Ok();
        }

        [HttpPost("payment")]
        public IActionResult Payment()
        {
            _logger.LogInformation("Payment endpoint is called.");

            return Ok();
        }

        [HttpPost("login")]
        public IActionResult Login()
        {
            _logger.LogInformation("Login endpoint is called.");

            return Ok(
                new CustomerSessionResponse(
                    new CustomerInformation(
                        new Contact(Guid.NewGuid().ToString())
                        )
                    )
                );
        }

        [HttpPost("create-customer/{id}/address/{addressId}/bind")]
        public IActionResult CreateCustomer(
            [FromRoute] string id,
            [FromRoute] string addressId,
            [FromQuery] string traceId,
            [FromQuery] string contactInformationId,
            [FromBody] CreateCustomerRequest request)
        {
            _logger.LogInformation("Create customer endpoint is called.");

            return Ok(new CreateCustomerResponse(
                id,
                addressId,
                request.Name,
                request.Surname,
                request.BirthYear,
                request.Nationality,
                contactInformationId,
                traceId)
                );
        }

        [HttpPost("create-account")]
        public IActionResult CreateAccount()
        {
            _logger.LogInformation("Create account endpoint is called.");

            return Ok();
        }
    }
}