using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Insightly.Models
{
    public class Chat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string OtherUserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey(nameof(OtherUserId))]
        public virtual ApplicationUser OtherUser { get; set; }

        public virtual List<ChatMessage> Messages { get; set; } = new();
    }
}
