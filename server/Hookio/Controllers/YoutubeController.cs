using Hookio.Database.Interfaces;
using Hookio.Youtube.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Hookio.Controllers
{
    [Route("/api/youtube")]
    [ApiController]
    public class YoutubeController(ILogger<YoutubeController> logger, IYoutubeService youtubeService, IDataManager dataManager) : ControllerBase
    {
        [HttpGet("callback")]
        [HttpPost("callback")]
        public ActionResult NotificationsCallback()
        {
            var request = HttpContext.Request;
            var token = request.Query.TryGetValue("hub.verify_token", out var verifyToken);
            if (!token || !youtubeService.VerifyToken(verifyToken!))
            {
                logger.LogInformation("Unathorized request from {Url} detected", request.Path);
                return Unauthorized(new
                {
                    Error = "Invalid verify_token"
                });
            }

            logger.LogInformation("Recieved {Method} notification from youtube", request.Method);
            var challenge = request.Query.TryGetValue("hub.challenge", out var challengeResponse);
            if (!challenge)
            {
                logger.LogInformation("Handling notification");
                var stream = request.Body;
                var data = youtubeService.ConvertAtomToSyndication(stream);
                
                // TODO: get subscription and appropriate event, send the message or edit, cache the message ID to update it in case of video update (for limited time, 12h maybe?)
                Console.WriteLine(data);
                return Ok();
            }
            else
            {
                // this is a subscription confirmation
                logger.LogInformation("handling subscription confirmation with challenge {Challenge}", challengeResponse!);
                return Ok(new
                {
                    Content = new StringContent(challengeResponse!),
                    ReasonPhrase = challenge,
                    StatusCode = HttpStatusCode.OK
                });
            }
        }
    }
}
