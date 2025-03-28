using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SillyChatBackend.DTOs
{
    public class RefreshRequest(string refreshToken)
    {
        [Required]
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = refreshToken;
    }
}