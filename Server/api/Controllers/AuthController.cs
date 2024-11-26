using Microsoft.AspNetCore.Mvc;
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
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly JwtTokenHelper _jwtTokenHelper;

        public AuthController(ApplicationDbContext dbContext, JwtTokenHelper jwtTokenHelper)
        {
            _dbContext = dbContext;
            _jwtTokenHelper = jwtTokenHelper;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] Register request)
        {
            string ipAddress = "";
            string userAgent = "";
            try
            {
                // Check if the username already exists
                if (_dbContext.Users.Any(u => u.Username == request.UserItem.Username))
                {
                    return Conflict(new { Message = "Username already exists." });
                }

                // Hash the password using BCrypt
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.UserItem.Password);

                // Replace the plain text password with the hashed password
                request.UserItem.Password = hashedPassword;

                // Add the user to the database
                _dbContext.Users.Add(request.UserItem);
                _dbContext.SaveChanges();
                var token = _jwtTokenHelper.GenerateJwtToken(request.UserItem.Username);

                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
                userAgent = Request.Headers["User-Agent"].ToString();
                Logger.LogRegister(request.UserItem.Username, ipAddress, userAgent);

                return Ok(new { Message = "Registration successful", Token = token });
            }
            catch(Exception ex)
            {
                Logger.ErrorUserLog(ex, request.UserItem.Username, ipAddress, userAgent);
                return StatusCode(500, "An unexpected error occurred.");
            }
            

        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Login request)
        {
            string ipAddress = "";
            string userAgent = "";
            try
            {
                // Find the user in the database
                var user = _dbContext.Users.FirstOrDefault(u => u.Username == request.UserItem.Username);

                // If the user doesn't exist, return an error
                if (user == null || !BCrypt.Net.BCrypt.Verify(request.UserItem.Password, user.Password))
                {
                    return Unauthorized(new { Message = "Invalid username or password." });
                }

                // Compare the entered password with the hashed password stored in the database
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.UserItem.Password, user.Password);

                // If the password is invalid, return an error
                if (!isPasswordValid)
                {
                    return Unauthorized(new { Message = "Invalid password" });
                }
                var token = _jwtTokenHelper.GenerateJwtToken(request.UserItem.Username);

                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
                userAgent = Request.Headers["User-Agent"].ToString();
                Logger.LogLogin(request.UserItem.Username, ipAddress, userAgent);

                // If login is successful, return a success message (or JWT token later)
                return Ok(new { Message = "Login successful", Token = token });
            }
            catch (Exception ex)
            {
                Logger.ErrorUserLog(ex, request.UserItem.Username, ipAddress, userAgent);
                return StatusCode(500, "An unexpected error occurred.");
            }
           
        }
    }
}