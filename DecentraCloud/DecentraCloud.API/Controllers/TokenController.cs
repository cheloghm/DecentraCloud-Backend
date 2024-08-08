using DecentraCloud.API.DTOs;
using DecentraCloud.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DecentraCloud.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly TokenHelper _tokenHelper;

        public TokenController(TokenHelper tokenHelper)
        {
            _tokenHelper = tokenHelper;
        }

        [HttpPost("verify")]
        [AllowAnonymous]
        public IActionResult VerifyToken([FromBody] TokenDto tokenDto)
        {
            var principal = _tokenHelper.VerifyToken(tokenDto.Token);

            if (principal == null)
            {
                return Unauthorized();
            }

            return Ok(new { message = "Token is valid" });
        }
    }
}
