using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SillyChatBackend.DTOs
{
    public class FriendRequestCreation
    {
        [Required]
        [JsonPropertyName("friend_id")]
        public uint FriendId { get; set; }
    }
}