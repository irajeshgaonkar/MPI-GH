using HCA.Infrastructure.Security.Tokens;

namespace HCA.Infrastructure.Security.Contracts;

public interface ITokenHandler
{
    AccessToken CreateAccessToken(string userName, List<string> roles);
}

