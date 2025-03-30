using SillyChatBackend.DTOs;
using SillyChatBackend.Models;
using SillyChatBackend.Repositories;

namespace SillyChatBackend.Services;

public interface IAuthService
{
    AuthResult Login(AuthData authData);
    AuthResult Signup(AuthData authData);
    FriendInfo? SearchUser(uint userId, string email);
}

public class AuthService(IUserRepository userRepository, ITokenService tokenService) : IAuthService
{
    public AuthResult Login(AuthData authData)
    {
        var user = userRepository.GetUserByEmail(authData.Email);
        if (user == null || user.Password != authData.Password)
        {
            return new AuthResult(false, "Invalid email or password");
        }
        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken(user);
        return new AuthResult(true, "Login successful", user.Id,  user.Username, accessToken, refreshToken);
    }

    public AuthResult Signup(AuthData authData)
    {
        var existingUser = userRepository.GetUserByEmail(authData.Email);
        if (existingUser != null)
        {
            return new AuthResult(false, "Email already exists");
        }
        User? user = new()
        {
            Username = authData.Username!,
            Email = authData.Email,
            Password = authData.Password
        };
        user = userRepository.CreateUser(user);
        return user == null ? new AuthResult(false, "Failed to create user") : new AuthResult(true, "Signup successful");
    }

    public FriendInfo? SearchUser(uint userId, string email)
    {
        var user = userRepository.GetUserByEmail(email);
        if (user == null)
        {
            return null;
        }
        return new FriendInfo(user.Id, user.Username, user.Email);
    }
}