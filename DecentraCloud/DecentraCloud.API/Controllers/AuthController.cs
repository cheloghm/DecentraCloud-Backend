using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DecentraCloud.API.DTOs;
using DecentraCloud.API.Interfaces.ServiceInterfaces;
using DecentraCloud.API.Helpers;
using System.Security.Claims;

namespace DecentraCloud.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly TokenHelper _tokenHelper;

        public AuthController(IUserService userService, TokenHelper tokenHelper)
        {
            _userService = userService;
            _tokenHelper = tokenHelper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto userRegisterDTO)
        {
            try
            {
                var user = await _userService.RegisterUser(userRegisterDTO);

                if (user == null)
                {
                    return BadRequest(new { message = "User already exists." });
                }

                var token = _tokenHelper.GenerateJwtToken(user);

                return Ok(new { token });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDTO)
        {
            try
            {
                var user = await _userService.AuthenticateUser(userLoginDTO);

                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid credentials." });
                }

                var token = _tokenHelper.GenerateJwtToken(user);

                return Ok(new { token });
            }
            catch (System.Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetUserDetails()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userDetails = await _userService.GetUserDetails(userId);
            if (userDetails == null) return NotFound();

            return Ok(userDetails);
        }


        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateUser([FromBody] UserDetailsDto userDetailsDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var updatedUser = await _userService.UpdateUser(userDetailsDto, userId);

            if (updatedUser == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(updatedUser);
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteUser()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _userService.DeleteUser(userId);
                if (!result) return NotFound();

                return NoContent();
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
