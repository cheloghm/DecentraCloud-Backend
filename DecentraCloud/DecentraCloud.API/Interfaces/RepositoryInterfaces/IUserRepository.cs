using DecentraCloud.API.Models;
using System.Threading.Tasks;

namespace DecentraCloud.API.Interfaces.RepositoryInterfaces
{
    public interface IUserRepository
    {
        Task<User> RegisterUser(User user);
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserById(string userId);
        Task UpdateUser(User user);
        Task<bool> DeleteUser(string userId);
        Task UpdateUserStorageUsage(string userId, long storageUsed);
    }
}
