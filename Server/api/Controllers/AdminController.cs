using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Server.Data; // Your database context
using Server.api.Controllers;

namespace Server.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Only users with the "Admin" role can access this route
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public AdminController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Admin access route to get all users (example)
        [HttpGet("all-users")]
        public IActionResult GetAllUsers()
        {
            var users = _dbContext.Users.ToList();
            return Ok(users);
        }
    }
}
