using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SillyChatBackend.Config;
using SillyChatBackend.DTOs;
using SillyChatBackend.Models;
using SillyChatBackend.Repositories;

namespace SillyChatBackend.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken(User user);
    TokenResult? RefreshTokens(string refreshToken);
}

public class TokenService(JwtSettings jwtSettings, IUserRepository userRepository) : ITokenService
{
    public string GenerateAccessToken(User user)
    {
        return GenerateToken(user, jwtSettings.AccessSecret, DateTime.UtcNow.AddMinutes(30));
    }

    public string GenerateRefreshToken(User user)
    {
        return GenerateToken(user, jwtSettings.RefreshSecret, DateTime.UtcNow.AddDays(7));
    }

    public TokenResult? RefreshTokens(string refreshToken) {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(refreshToken);
        var userId = uint.Parse(token.Subject);
        var user = userRepository.GetUserById(userId);
        if (user == null)
        {
            return null;
        }
        return new TokenResult(GenerateAccessToken(user), GenerateRefreshToken(user));
    }

    private static string GenerateToken(User user, string secret, DateTime expiry)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}