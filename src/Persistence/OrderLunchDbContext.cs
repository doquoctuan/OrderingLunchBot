using Microsoft.EntityFrameworkCore;
using OrderRice.Entities;
using OrderRice.Extentions;

namespace OrderRice.Persistence
{
    public class OrderLunchDbContext : DbContext
    {
        public OrderLunchDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Users> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // CosmosDb specifics
            modelBuilder.Entity<Users>()
                        .ToContainer("Users")
                        .HasPartitionKey(x => x.Department)
                        .HasKey(o => o.UserName);

            base.OnModelCreating(modelBuilder);
        }

    }
}
