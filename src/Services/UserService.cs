using Dapper;
using OrderLunch.Interfaces;
using OrderLunch.Models;
using UTC2_Tool.Context;

namespace OrderLunch.Services
{
    public class UserService(DapperContext dapperContext) : IUserService
    {

        //private readonly OrderLunchDbContext _dbContext;

        //public UserService(OrderLunchDbContext dbContext)
        //{
        //    _dbContext = dbContext;
        //}

        //public async Task<Users> CreateUser(Users user)
        //{
        //    _dbContext.Users.Add(user);
        //    await _dbContext.SaveChangesAsync();
        //    return user;
        //}

        //public bool DeleteUser(string userName)
        //{
        //    throw new NotImplementedException();
        //}

        //public List<Users> FindByFilter(Func<Users, bool> predicate)
        //{
        //    return _dbContext.Users.Where(predicate).ToList();
        //}

        //public async Task<Users> FindByUserName(string userName)
        //{
        //    return await _dbContext.Users.FindAsync(userName);
        //}

        //public List<Users> GetList()
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<List<UserSearchDTO>> Search(string keyword)
        {
            string query = @"
                select ""staffCode"", ""userName"", ""staffName"", ""email"", ""avatar"", ""phoneNumber""
                from search_users_by_term(:keyword, :limit_number)
            ";

            using var connection = dapperContext.CreateConnection();
            var parameters = new { keyword, limit_number = 1 };
            var result = await connection.QueryAsync<UserSearchDTO>(query, parameters);
            return result.ToList();
        }

        //public async Task<Users> UpdateUser(Users user)
        //{
        //    _dbContext.Users.Update(user);
        //    await _dbContext.SaveChangesAsync();
        //    return user;
        //}
    }
}
