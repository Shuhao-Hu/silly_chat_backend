using System.ComponentModel.DataAnnotations;

namespace SillyChatBackend.Models
{
    public class User : BaseEntity
    {
        [Key]
        public uint Id { get; set; }

        [Required]
        [MaxLength(30)]
        public required string Username { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(30)]
        public required string Email { get; set; }
    
        [Required]
        [MaxLength(30)]
        public required string Password { get; set; }
    }
}