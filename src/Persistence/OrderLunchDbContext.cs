using Microsoft.EntityFrameworkCore;
using OrderRice.Entities;

namespace OrderRice.Persistence
{
    public class OrderLunchDbContext : DbContext
    {
        public OrderLunchDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Users> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Users>()
                .ToTable("Users")
                .HasKey(x => x.UserName);

            base.OnModelCreating(modelBuilder);
        }
    }
}
