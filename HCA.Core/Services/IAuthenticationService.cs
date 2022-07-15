using HCA.Models;

namespace HCA.Core.Services;

public interface IAuthenticationService
{
    Task<UserTokenModel?> CreateAccessTokenAsync(string email, string password);
}

