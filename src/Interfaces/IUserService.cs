using OrderLunch.Models;

namespace OrderLunch.Interfaces
{
    public interface IUserService
    {
        //List<Users> GetList();
        //Task<Users> FindByUserName(string userName);
        //List<Users> FindByFilter(Func<Users, bool> predicate);
        //Task<Users> CreateUser(Users user);
        //Task<Users> UpdateUser(Users user);
        //bool DeleteUser(string userName);
        Task<List<UserSearchDTO>> Search(string keyword);
    }
}
