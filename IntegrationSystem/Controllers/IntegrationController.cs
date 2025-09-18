using IntegrationSystem.Models;
using IntegrationSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationSystem.Controllers
{
    [ApiController]
    [Route("api/integration")]
    public class IntegrationController : ControllerBase
    {
        private readonly IntegrationOrchestrator _orchestrator;

        public IntegrationController(IntegrationOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpPost("orders")]
        public async Task<IActionResult> ProcessOrder([FromBody] OrderModel order)
        {
            if (order is null || order.Id <= 0 || order.ProductId <= 0 || string.IsNullOrWhiteSpace(order.ProductName) || order.Quantity <= 0)
                return BadRequest(new { message = "Invalid order payload." });

            await _orchestrator.ProcessOrderAsync(order);
            return Accepted(new { message = "Order accepted for processing." });
        }
    }
}
