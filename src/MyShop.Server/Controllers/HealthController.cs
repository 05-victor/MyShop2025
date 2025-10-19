using Microsoft.AspNetCore.Mvc;

namespace MyShop.Server.Controllers
{
    /// <summary>
    /// Health check controller for monitoring application status
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Basic health check endpoint
        /// </summary>
        /// <returns>OK status if application is healthy</returns>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "MyShop.Server"
            });
        }
    }
}