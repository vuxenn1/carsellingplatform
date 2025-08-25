using Microsoft.AspNetCore.Mvc;
using CARDB_EF.Services;
using CARDB_EF.Models.EF;
using CARDB_EF.Models.DTOs;

namespace CARDB_EF.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly UserService userService;

        public UserController(UserService userService)
        {
            this.userService = userService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var token = userService.ValidateLogin(request.Username, request.Password);
            if (token != null)
                return Ok(new { token = token });

            return Unauthorized("Invalid credentials or deactivated account.");
        }

        [HttpPost("register")]
        public IActionResult CreateUser([FromBody] RegisteredUser user)
        {
            if (userService.CreateUser(user, out var error))
            {
                var token = userService.ValidateLogin(user.Username, user.Password);
                if (token != null)
                    return Ok(new { token = token });
            }
            return BadRequest(error ?? "Invalid user data or user may already exist.");
        }

        [HttpGet("all")]
        public IActionResult GetAllUsers()
        {
            var users = userService.GetAllUsers();

            return Ok(users);
        }

        [HttpGet("profile/{userId}")]
        public IActionResult GetUserProfile(int userId)
        {
            var user = userService.GetUserProfile(userId);
            if (user == null)
                return NotFound("User not found.");

            return Ok(user);

        }

        [HttpPut("edit/{userId}")]
        public IActionResult EditUser(int userId, [FromBody] UserUpdateDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid payload.");

            if (userService.EditUser(userId, dto, out var error))
                return Ok(new { success = true });

            return BadRequest(error ?? "Update failed.");
        }

        [HttpPut("deactivate/{userId}")]
        public IActionResult DeactivateUser(int userId)
        {
            if (userService.DeactivateUser(userId))
                return Ok("User deactivated.");

            return NotFound("User not found.");
        }

        [HttpPut("activate/{userId}")]
        public IActionResult ActivateUser(int userId)
        {
            if (userService.ActivateUser(userId))
                return Ok("User activated.");

            return NotFound("User not found.");
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}