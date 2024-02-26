using Microsoft.Azure.Cosmos;
using OrderRice.Entities;
using OrderRice.Persistence;

namespace OrderRice.Extentions
{
    public static class OrderLunchDbContextSeed
    {
        public static void SeedDataFromGoogleSheetAsync(OrderLunchDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            context.Users.Add(new Users
            {
                Id = Guid.NewGuid(),
                UserName = "tuandq16",
                FullName = "Đỗ Quốc Tuấn",
                Department = "1"
            });
            context.SaveChanges();
        }
    }
}
