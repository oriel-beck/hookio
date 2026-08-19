using Hookio.Shared;
using Hookio.Twitch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hookio.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/twitch/eventsub")]
public class TwitchEventSubController(TwitchEventSubHandler handler) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Handle(CancellationToken cancellationToken)
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync(cancellationToken);
        Request.Body.Position = 0;

        var secret = Environment.GetEnvironmentVariable(EnvNames.TwitchEventSubSecret);
        var messageId = Request.Headers[TwitchEventSubSignature.MessageIdHeader].ToString();
        var timestamp = Request.Headers[TwitchEventSubSignature.TimestampHeader].ToString();
        var signature = Request.Headers[TwitchEventSubSignature.SignatureHeader].ToString();
        var messageType = Request.Headers[TwitchEventSubSignature.MessageTypeHeader].ToString();

        if (!TwitchEventSubSignature.IsValid(secret ?? "", messageId, timestamp, body, signature))
            return StatusCode(StatusCodes.Status403Forbidden);

        if (string.Equals(messageType, "webhook_callback_verification", StringComparison.OrdinalIgnoreCase))
        {
            using var doc = System.Text.Json.JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("challenge", out var challenge))
                return Content(challenge.GetString() ?? "", "text/plain");
            return BadRequest();
        }

        if (string.Equals(messageType, "notification", StringComparison.OrdinalIgnoreCase))
            await handler.HandleNotificationAsync(body, cancellationToken);

        return NoContent();
    }
}
