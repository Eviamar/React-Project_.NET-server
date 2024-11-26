using Microsoft.EntityFrameworkCore;
using Server.api.auth;

namespace Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Define your tables as DbSet<T> properties. Example:
        public DbSet<Register> Users { get; set; }
    }
}