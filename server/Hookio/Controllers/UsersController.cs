using Hookio.Contracts;
using Hookio.Database.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hookio.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class UsersController(IDataManager dataManager) : ControllerBase
    {
        /// <summary>
        /// Returns the current user based on the Authorization token
        /// </summary>
        /// <returns code="200">The current user</returns>
        /// <returns code="401">You are not authorized</returns>
        [Authorize]
        [HttpGet("current")]
        public async Task<ActionResult<CurrentUserResponse>> GetCurrentUser()
        {
            var idClaim = HttpContext.User.Claims.First(claim => claim.Type == "id");
            _ = ulong.TryParse(idClaim.Value, out var userId);
            return Ok(await dataManager.GetUser(userId));
        }

        /// <summary>
        /// Logs out the current user
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Authenticates a user based on the auth code returned from discord
        /// </summary>
        /// <param name="code"></param>
        /// <returns>Current user</returns>
        [HttpPost("authenticate/{code}")]
        public async Task<ActionResult<CurrentUserResponse?>> Authenticate(string code) =>
            Ok(await dataManager.Authenticate(HttpContext, code));
    }
}
