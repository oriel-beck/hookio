using Microsoft.AspNetCore.Mvc;
using Hookio.Contracts;

namespace Hookio.Controllers
{
    [Route("/api/youtube")]
    [ApiController]
    public class YTController(ILogger<YTController> logger) : ControllerBase
    {
        [HttpPost("/subscribe")]
        public IActionResult Subscribe([FromBody] YTSubscribeRequest request)
        {
            return Ok(request);
        }
    }
}
