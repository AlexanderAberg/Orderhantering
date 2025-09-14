using IntegrationSystem.Models;
using IntegrationSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IntegrationSystem.Controllers
{
    [ApiController]
    [Route("api/integrate")]
    public class IntegrationController : ControllerBase
    {
        private readonly IntegrationOrchestrator _orchestrator;

        public IntegrationController(IntegrationOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok(new { Status = "Allt OK" });
        }

        [HttpPost("order")]
        public async Task<IActionResult> ProcessOrder([FromBody] OrderModel order)
        {
            if (string.IsNullOrWhiteSpace(order.ProductName) || order.Quantity <= 0)
                return BadRequest(new { message = "Ogiltigt orderdata." });

            await _orchestrator.ProcessOrderAsync(order);
            return Ok(new { message = "Order integrerad och processad." });
        }
    }
}
