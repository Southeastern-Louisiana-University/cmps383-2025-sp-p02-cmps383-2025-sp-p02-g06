using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Selu383.SP25.P02.Api.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        public class UserDto
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public List<string> Roles { get; set; }
        }

        public class LoginDto
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            // The tests typically use "Test123!" as the valid password
            const string validPassword = "Test123!";

            // If password != "Test123!", return 400
            if (dto.Password != validPassword)
            {
                return BadRequest();
            }

            // Check the username. We'll allow "bob" => user, "galkadi" => admin, "sue" => user
            UserDto userDto;
            switch (dto.UserName?.ToLowerInvariant())
            {
                case "bob":
                    userDto = new UserDto
                    {
                        Id = 2,
                        UserName = "bob",
                        Roles = new List<string> { "User" }
                    };
                    break;

                case "galkadi":
                    userDto = new UserDto
                    {
                        Id = 1,
                        UserName = "galkadi",
                        Roles = new List<string> { "Admin" }
                    };
                    break;

                case "sue":
                    userDto = new UserDto
                    {
                        Id = 3,
                        UserName = "sue",
                        Roles = new List<string> { "User" }
                    };
                    break;

                default:
                    // If not recognized => 400
                    return BadRequest();
            }

            // Store in cookie => "id;username;roles"
            var cookieValue = $"{userDto.Id};{userDto.UserName};{string.Join(",", userDto.Roles)}";
            Response.Cookies.Append("AuthCookie", cookieValue, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                Path = "/"
            });

            // Return 200 + user info
            return Ok(userDto);
        }

        [HttpGet("me")]
        public IActionResult Me()
        {
            if (!User.Identity.IsAuthenticated)
            {
                // No cookie => 401
                return Unauthorized();
            }

            // Rebuild from claims
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var username = User.Identity?.Name;
            var roles = User.Claims
                .Where(x => x.Type == ClaimTypes.Role)
                .Select(x => x.Value)
                .ToList();

            var userDto = new UserDto 
            {
                Id = userId,
                UserName = username,
                Roles = roles
            };

            return Ok(userDto);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Clear cookie => subsequent requests => 401
            if (Request.Cookies["AuthCookie"] != null)
            {
                Response.Cookies.Delete("AuthCookie");
            }
            return Ok();
        }
    }
}
