using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SillyChatBackend.Models
{
    public class Message : BaseEntity
    {
        [Key]
        public uint Id { get; set; }

        [Required]
        public uint SenderId { get; set; }

        [ForeignKey("SenderId")]
        public required User User { get; set; }

        [Required]
        public uint RecipientId { get; set; }

        [ForeignKey("RecipientId")]
        public required User Recipient { get; set; }

        [Required]
        public required string Content { get; set; }

        [Required]
        public bool Read { get; set; } = false;
    }
}