using Microsoft.EntityFrameworkCore;
using OrderRice.Entities;
using System.Reflection;

namespace OrderRice.Persistence
{
    public class OrderLunchDbContext : DbContext
    {
        public OrderLunchDbContext()
        {
        }

        public OrderLunchDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseCosmos(
            "",
            "",
            databaseName: "OrderLunchDb");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // CosmosDb specifics
            modelBuilder.Entity<Users>()
                .ToContainer("Users");

            modelBuilder.Entity<Users>()
                .HasKey(o => o.UserName);

            base.OnModelCreating(modelBuilder);
        }

    }
}
