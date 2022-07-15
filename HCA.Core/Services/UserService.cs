using System;
using HCA.Data.Entities;
using HCA.Data.Repository;

namespace HCA.Core.Services
{

    public interface IUserService
    {
        Task<UserEntity?> FindByEmailAsync(string email);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserEntity?> FindByEmailAsync(string email)
        {
            var user = await _userRepository.FindByEmail(email);

            if(null == user)
            {
                return null;
            }

            var roles = await _userRepository.GetRoles();
            var role = roles.FirstOrDefault(t => t.Id == user.UserRoles.First().RoleId);

            user.UserRoles.First().Role = role;

            if (user == null)
                return null;

            return user;
        }
    }
}

