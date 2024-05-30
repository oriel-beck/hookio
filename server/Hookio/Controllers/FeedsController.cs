using Hookio.Contracts;
using Hookio.Feeds;
using Microsoft.AspNetCore.Mvc;

namespace Hookio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedsController(IHttpClientFactory httpClientFactory) : ControllerBase
    {
        /// <summary>
        /// Returns the template strings that are useable in the RSS feed
        /// </summary>
        /// <param name="url"></param>
        /// <returns code="200">Valid RSS feed was provided</returns>
        /// <returns code="400">Invalid RSS feed or Invalid URL was provided</returns>
        [HttpGet("{url}")]
        public async Task<ActionResult<List<TemplateStringResponse>>> GetTemplateStrings(string url)
        {
            var isUrl = url.StartsWith("http") && Uri.TryCreate(url, UriKind.Absolute, out var _);
            if (!isUrl) return BadRequest();
            var templateStrings = await FeedUtils.Parse(url, httpClientFactory.CreateClient());
            if (templateStrings.Item2.Id == null || (templateStrings.Item2.Updated == null && templateStrings.Item2.Published == null)) return BadRequest(new
            {
                Error = "Invalid RSS feed"
            });
            return Ok(templateStrings.Item1);
        }
    }
}
