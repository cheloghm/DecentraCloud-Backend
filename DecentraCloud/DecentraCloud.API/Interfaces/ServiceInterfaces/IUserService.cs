using DecentraCloud.API.DTOs;
using DecentraCloud.API.Models;
using System.Threading.Tasks;

namespace DecentraCloud.API.Interfaces.ServiceInterfaces
{
    public interface IUserService
    {
        Task<User> RegisterUser(UserRegistrationDto userDto);
        Task<User> AuthenticateUser(UserLoginDto userDto);
        Task<User> GetUserById(string userId);
        Task<User> UpdateUser(UserDetailsDto userDto, string userId);
        Task<bool> DeleteUser(string userId);
    }
}
