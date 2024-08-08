using AutoMapper;
using DecentraCloud.API.DTOs;
using DecentraCloud.API.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DecentraCloud.API.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Add mappings here
            CreateMap<User, UserRegistrationDto>();
            CreateMap<UserRegistrationDto, User>();
        }
    }
}
