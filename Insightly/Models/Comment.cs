using System.ComponentModel.DataAnnotations;

namespace Insightly.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string AuthorId { get; set; } =string.Empty;
    }
}
