using OrderLunch.Entities;
using OrderLunch.Interfaces;
using OrderLunch.Persistence;

namespace OrderLunch.Services
{
    public class UserService : IUserService
    {
        private readonly OrderLunchDbContext _dbContext;

        public UserService(OrderLunchDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Users> CreateUser(Users user)
        {
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public bool DeleteUser(string userName)
        {
            throw new NotImplementedException();
        }

        public List<Users> FindByFilter(Func<Users, bool> predicate)
        {
            return _dbContext.Users.Where(predicate).ToList();
        }

        public async Task<Users> FindByUserName(string userName)
        {
            return await _dbContext.Users.FindAsync(userName);
        }

        public List<Users> GetList()
        {
            throw new NotImplementedException();
        }

        public async Task<Users> UpdateUser(Users user)
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }
    }
}
