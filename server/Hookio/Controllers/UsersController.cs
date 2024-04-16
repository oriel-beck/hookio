using Hookio.Contracts;
using Hookio.Database.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hookio.Controllers
{
    [Route("/api/users")]
    [ApiController]
    public class UsersController(IDataManager dataManager) : ControllerBase
    {
        [Authorize]
        [HttpGet("current")]
        public async Task<ActionResult<CurrentUserResponse>> GetCurrentUser()
        {
            var idClaim = HttpContext.User.Claims.First(claim => claim.Type == "id");
            _ = ulong.TryParse(idClaim.Value, out var userId);
            return Ok(await dataManager.GetUser(userId));
        }

        [HttpPost("logout")]
        public IActionResult LogOut()
        {
            HttpContext.Response.Cookies.Append("Authorization", "", new()
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow,
                SameSite = SameSiteMode.Strict
            });
            return NoContent();
        }


        [HttpPost("authenticate/{code}")]
        public async Task<ActionResult<CurrentUserResponse?>> Authenticate(string code) =>
            Ok(await dataManager.Authenticate(HttpContext, code));
    }
}
