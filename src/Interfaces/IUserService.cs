using OrderRice.Entities;

namespace OrderRice.Interfaces
{
    public interface IUserService
    {
        List<Users> GetList();
        Task<Users> FindByUserName(string userName);
        List<Users> FindByFilter(Func<Users, bool> predicate);
        Task<Users> CreateUser(Users user);
        Task<Users> UpdateUser(Users user);
        bool DeleteUser(string userName);
    }
}
