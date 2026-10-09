using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HCA.Infrastructure.Security.Contracts;

namespace HCA.Infrastructure.Security.Tokens;

public class TokenHandler : ITokenHandler
{
    private readonly ISet<RefreshToken> _refreshTokens = new HashSet<RefreshToken>();

    private readonly TokenOptions _tokenOptions;

    private readonly SigningConfigurations _signingConfigurations;

    private readonly IPasswordHasher _passwordHaser;

    public TokenHandler(TokenOptions tokenOptions, SigningConfigurations signingConfigurations, IPasswordHasher passwordHaser)
    {
        _passwordHaser = passwordHaser;
        _tokenOptions = tokenOptions;
        _signingConfigurations = signingConfigurations;
    }

    public AccessToken CreateAccessToken(string userName, List<string> roles)
    {
        var refreshToken = BuildRefreshToken();
        var accessToken = BuildAccessToken(userName, roles, refreshToken);
        _refreshTokens.Add(refreshToken);
        return accessToken;
    }

    private RefreshToken BuildRefreshToken()
    {
        var refreshToken = new RefreshToken
        (
            token: _passwordHaser.HashPassword(Guid.NewGuid().ToString()),
            expiration: DateTime.UtcNow.AddSeconds(_tokenOptions.RefreshTokenExpiration).Ticks
        );

        return refreshToken;
    }

    private AccessToken BuildAccessToken(string userName, List<string> roles, RefreshToken refreshToken)
    {
        var accessTokenExpiration = DateTime.UtcNow.AddSeconds(_tokenOptions.AccessTokenExpiration);

        var securityToken = new JwtSecurityToken
        (
            issuer: _tokenOptions.Issuer,
            audience: _tokenOptions.Audience,
            claims: GetClaims(userName, roles),
            expires: accessTokenExpiration,
            notBefore: DateTime.UtcNow,
            signingCredentials: _signingConfigurations.SigningCredentials
        );

        var handler = new JwtSecurityTokenHandler();
        var accessToken = handler.WriteToken(securityToken);

        return new AccessToken(accessToken, accessTokenExpiration.Ticks, refreshToken);
    }

    private IEnumerable<Claim> GetClaims(string userName, List<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, userName)
        };

        foreach (var userRole in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole));
        }

        return claims;
    }
}
