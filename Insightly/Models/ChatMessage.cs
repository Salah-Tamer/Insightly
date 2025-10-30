using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing.Printing;

namespace Insightly.Models
{
    public class ChatMessage
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Message { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        [Required]
        public int ChatId { get; set; }
        [ForeignKey("ChatId")]
        public virtual Chat Chat { get; set; }
        public string SenderId { get; set; }
        [ForeignKey("SenderId")]
        public virtual ApplicationUser Sender { get; set; }
        

    }
}
