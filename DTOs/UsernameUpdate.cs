using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SillyChatBackend.DTOs;

public class UsernameUpdate
{
    [Required]
    [JsonPropertyName("username")]
    public required string Username { get; set; }
}