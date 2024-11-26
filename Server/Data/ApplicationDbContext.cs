using Microsoft.EntityFrameworkCore;
using Server.api.Models;

namespace Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Define your tables as DbSet<T> properties. Example:
        public DbSet<UserItem> Users { get; set; }
    }
}