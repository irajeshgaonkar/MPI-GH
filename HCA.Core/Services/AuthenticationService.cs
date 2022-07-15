using System;
using HCA.Infrastructure.Security.Contracts;
using HCA.Models;

namespace HCA.Core.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserService _userService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenHandler _tokenHandler;

    public AuthenticationService(IUserService userService, IPasswordHasher passwordHasher, ITokenHandler tokenHandler)
    {
        _tokenHandler = tokenHandler;
        _passwordHasher = passwordHasher;
        _userService = userService;
    }

    public async Task<UserTokenModel?> CreateAccessTokenAsync(string email, string password)
    {
        var user = await _userService.FindByEmailAsync(email);

        //if (user == null || !_passwordHasher.PasswordMatches(password, user.Password))
        //{
        //    return null;
        //}

        if (user == null || password != user.Password)
        {
            return null;
        }

        var token = _tokenHandler.CreateAccessToken(user.Email, user.UserRoles.Select(r => r.Role.Name).ToList());

        var userRole = user.UserRoles[0];
        var userToken = new UserTokenModel
        {
            Token = token.Token,
            Expiration = token.Expiration,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MiddleName = user.MiddleName ?? "",
            Role = new UserRoleModel() { Id = userRole.Role.Id, Name = userRole.Role.Name }
        };

        return userToken;
    }

}

