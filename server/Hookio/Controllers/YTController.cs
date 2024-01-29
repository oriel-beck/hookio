using Microsoft.AspNetCore.Mvc;
using Hookio.Contracts;
using Hookio.Database.Interfaces;

namespace Hookio.Controllers
{
    [Route("/api/youtube")]
    [ApiController]
    public class YTController(ILogger<YTController> logger, IDataManager dataManager) : ControllerBase
    {
        [HttpPost("/subscribe")]
        public IActionResult Subscribe([FromBody] YTSubscribeRequest request)
        {
            return Ok(request);
        }
    }
}
