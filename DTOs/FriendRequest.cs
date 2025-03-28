using System.Text.Json.Serialization;

namespace SillyChatBackend.DTOs
{
    public class FriendRequest(uint id, uint senderId, string username, string email)
    {
        [JsonPropertyName("friend_request_id")]
        public uint Id { get; set; } = id;

        [JsonPropertyName("sender_id")]
        public uint SenderId { get; set; } = senderId;

        [JsonPropertyName("sender_username")]
        public string SenderUsername { get; set; } = username;
        
        [JsonPropertyName("sender_email")]
        public string SenderEmail { get; set; } = email;
    }
}