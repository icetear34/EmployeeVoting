using Microsoft.AspNetCore.Mvc;

namespace EmployeeVoting.Api.Controllers
{
    /// <summary>
    /// 健康檢查控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// 健康檢查端點
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new 
            { 
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }
    }
}
