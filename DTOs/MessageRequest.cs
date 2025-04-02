using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SillyChatBackend.DTOs;

public class MessageRequest
{
    [Required]
    [JsonPropertyName("recipient_id")]
    public uint RecipientId { get; set; }
    
    [Required]
    [JsonPropertyName("content")]
    public required string Content { get; set; }
}