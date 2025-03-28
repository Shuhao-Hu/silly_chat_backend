using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SillyChatBackend.Models
{
    public class Friend : BaseEntity 
    {
        [Key]
        public uint Id { get; set; }

        [Required]
        public uint UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        public uint FriendId { get; set; }

        [ForeignKey("FriendId")]
        public User? FriendUser { get; set; }

        [Required]
        public required string Status { get; set; } = "pending";
    }
}