using OrderRice.Entities;

namespace OrderRice.Interfaces
{
    public interface IUserService
    {
        List<Users> GetList();
        Users FindByUserName();
        Task<Users> CreateUser(Users user);
        Users UpdateUser(Users user);
        bool DeleteUser(string userName);
    }
}
