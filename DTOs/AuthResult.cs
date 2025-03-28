using System.Text.Json.Serialization;

namespace SillyChatBackend.DTOs
{
    public class AuthResult(bool success, string message, uint userId = 0, string? username = null, string? accessToken = null, string? refreshToken = null)
    {
        public bool Success { get; set; } = success;

        public string? Message { get; set; } = message;

        [JsonPropertyName("id")]
        public uint? UserId { get; set; } = userId;

        public string? Username { get; set; } = username;

        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; } = accessToken;

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; } = refreshToken;
    }
}