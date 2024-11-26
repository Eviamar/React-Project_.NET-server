using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Server.Data; 
using Server.api.Controllers;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Server.Utils;
using Server.api.Models;

namespace Server.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // This ensures that only authenticated users can access these routes
    public class ProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public ProfileController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Get the user's profile information (secured route)
        [HttpGet("my-profile")]
        public IActionResult GetMyProfile()
        {
            string? username = User.Identity?.Name; // This gets the username from the JWT token
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { Message = "Username is missing or invalid." });
            }

            var user = _dbContext.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }
            
            return Ok(new { Username = user.Username, Email = user.Email, FullName = user.FullName });
        }

        // Update the user's profile information (secured route)
        [HttpPut("update-profile")]
        public IActionResult UpdateProfile([FromBody] UpdateProfile request)
        {
            string? username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { Message = "Username is missing or invalid." });
            }
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            // Update the user profile (assuming UpdateProfileRequest is a model containing profile fields)
            user.Email = request.UserItem.Email;
            user.FullName = request.UserItem.FullName;
            _dbContext.SaveChanges();

            return Ok(new { Message = "Profile updated successfully." });
        }
       

    }
}
