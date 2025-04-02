using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SillyChatBackend.Models
{
    public class Message : BaseEntity
    {
        [Key]
        public uint Id { get; set; }

        [Required]
        [JsonPropertyName("sender_id")]
        public uint SenderId { get; set; }

        [ForeignKey("SenderId")]
        public User? User { get; set; }

        [Required]
        [JsonPropertyName("recipient_id")]
        public uint RecipientId { get; set; }

        [ForeignKey("RecipientId")]
        public User? Recipient { get; set; }

        [Required]
        [MaxLength(300)]
        [JsonPropertyName("content")]
        public required string Content { get; set; }

        [Required]
        [JsonPropertyName("read")]
        public bool Read { get; set; } = false;
    }
}