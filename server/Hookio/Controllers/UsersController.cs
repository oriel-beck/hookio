using Hookio.Contracts;
using Hookio.Database.Interfaces;
using Hookio.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hookio.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController(IUserAuthService authService) : ControllerBase
    {
        [Authorize]
        [HttpGet("current")]
        public async Task<ActionResult<CurrentUserResponse>> GetCurrentUser()
        {
            var idClaim = HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == AuthConstants.IdClaim);
            if (idClaim is null || !ulong.TryParse(idClaim.Value, out var userId)) return Unauthorized();
            var user = await authService.GetUser(userId);
            return user is null ? Unauthorized() : Ok(user);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Append(AuthConstants.CookieName, "", Util.AuthCookieOptions(HttpContext, DateTimeOffset.UtcNow));
            return NoContent();
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<CurrentUserResponse>> Authenticate([FromQuery] string code)
        {
            var user = await authService.Authenticate(HttpContext, code);
            return user is null ? Unauthorized() : Ok(user);
        }
    }
}
