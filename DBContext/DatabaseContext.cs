using Microsoft.EntityFrameworkCore;
using CRM.Models;

namespace CRM.DBContext
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<Comments> Comments { get; set; }
    }
}
