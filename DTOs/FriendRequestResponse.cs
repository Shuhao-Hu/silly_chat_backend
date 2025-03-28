using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SillyChatBackend.DTOs
{
    public class FriendRequestResponse(uint senderId, string response)
    {
        [Required]
        [JsonPropertyName("sender_id")]
        public uint SenderId { get; set; } = senderId;

        [Required]
        [JsonPropertyName("response")]
        public string Response { get; set; } = response;
    }
}