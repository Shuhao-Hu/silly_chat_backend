using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SillyChatBackend.Config;

namespace SillyChatBackend.Utils;

public interface IUserContext
{
    uint? ExtractUserId();
    uint? GetSubjectFromToken(string token);
}

public class UserContext(IHttpContextAccessor httpContextAccessor, JwtSettings jwtSettings) : IUserContext
{
    public uint? ExtractUserId()
    {
        var idString = httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                       ?? httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (idString == null)
        {
            return null;
        }

        if (uint.TryParse(idString, out var userId))
        {
            return userId;
        }

        return null;
    }
    
    public uint? GetSubjectFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            // Decode and validate the token
            var key = Encoding.UTF8.GetBytes(jwtSettings.AccessSecret);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            // Validate and parse the token
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

            var idClaim = principal?.FindFirst(ClaimTypes.NameIdentifier) 
                          ?? principal?.FindFirst(JwtRegisteredClaimNames.Sub);

            if (idClaim != null)
            {
                if (uint.TryParse(idClaim.Value, out var userId))
                {
                    return userId;
                }
                return null;
            }
            else
            {
                throw new Exception("Sub claim not found in token.");
            }
        }
        catch (Exception)
        {
            return null; // Return null or handle exceptions based on your needs
        }
    }
}