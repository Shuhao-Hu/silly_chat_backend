using System.ComponentModel.DataAnnotations;

namespace SillyChatBackend.DTOs
{
    public class AuthData(string email, string password, string? username)
    {
        public string? Username { get; set; } = username;
        [Required]
        public required string Email { get; set; } = email;
        [Required]
        public required string Password { get; set; } = password;
    }
}