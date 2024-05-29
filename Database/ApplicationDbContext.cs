using BaseCrud.Entites;
using Microsoft.EntityFrameworkCore;

namespace BaseCrud.Database
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users;
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
        }
    }
}
