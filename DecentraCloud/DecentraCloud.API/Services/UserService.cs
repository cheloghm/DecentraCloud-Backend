using DecentraCloud.API.DTOs;
using DecentraCloud.API.Helpers;
using DecentraCloud.API.Interfaces.RepositoryInterfaces;
using DecentraCloud.API.Interfaces.ServiceInterfaces;
using DecentraCloud.API.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DecentraCloud.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly TokenHelper _tokenHelper;
        private readonly EncryptionHelper _encryptionHelper;
        private readonly string STORAGE_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage");

        public UserService(IUserRepository userRepository, TokenHelper tokenHelper, EncryptionHelper encryptionHelper)
        {
            _userRepository = userRepository;
            _tokenHelper = tokenHelper;
            _encryptionHelper = encryptionHelper;
        }

        public async Task<User> RegisterUser(UserRegistrationDto userDto)
        {
            var existingUser = await _userRepository.GetUserByEmail(userDto.Email);
            if (existingUser != null)
            {
                throw new Exception("User with this email already exists");
            }

            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                Password = _encryptionHelper.HashPassword(userDto.Password),
                Settings = new UserSettings
                {
                    ReceiveNewsletter = false,
                    Theme = "light"
                }
            };

            await _userRepository.RegisterUser(user);

            // Generate token for the user
            var token = _tokenHelper.GenerateJwtToken(user);
            user.Token = token;

            // Create user directory for storage
            string userStoragePath = Path.Combine(STORAGE_DIR, user.Id.ToString());
            if (!Directory.Exists(userStoragePath))
            {
                Directory.CreateDirectory(userStoragePath);
            }

            return user;
        }

        public async Task<User> AuthenticateUser(UserLoginDto userDto)
        {
            var user = await _userRepository.GetUserByEmail(userDto.Email);
            if (user == null || !_encryptionHelper.VerifyPassword(userDto.Password, user.Password))
            {
                throw new Exception("Invalid email or password");
            }

            return user;
        }

        public async Task<User> GetUserById(string userId)
        {
            return await _userRepository.GetUserById(userId);
        }
        public async Task<UserDetailsDto> GetUserDetails(string userId)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user == null) return null;

            return new UserDetailsDto
            {
                Username = user.Username ?? "No Name",
                Email = user.Email,
                Settings = user.Settings != null ? new UserSettingsDto
                {
                    ReceiveNewsletter = user.Settings.ReceiveNewsletter,
                    Theme = user.Settings.Theme
                } : new UserSettingsDto
                {
                    ReceiveNewsletter = false,
                    Theme = "light"
                },
                AllocatedStorage = user.AllocatedStorage,
                UsedStorage = user.UsedStorage
            };
        }

        public async Task<User> UpdateUser(UserDetailsDto userDto, string userId)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            user.Username = userDto.Username;
            user.Email = userDto.Email;
            user.Settings = new UserSettings
            {
                ReceiveNewsletter = userDto.Settings.ReceiveNewsletter,
                Theme = userDto.Settings.Theme
            };

            await _userRepository.UpdateUser(user);
            return user;
        }

        public async Task<bool> DeleteUser(string userId)
        {
            return await _userRepository.DeleteUser(userId);
        }
    }
}
