using System.ComponentModel.DataAnnotations;
using System.Drawing.Printing;

namespace Insightly.Models
{
    public class ChatMessage
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string SenderId { get; set; }
        [Required]
        public string ReceiverId { get; set; }
        [Required]
        public string Message { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

    }
}
