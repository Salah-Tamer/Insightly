using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Insightly.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [Required]
        public DateTime? UpdatedAt { get; set; }
        
        [Required]
        public int AuthorId { get; set; }
        
        [Required]
        public int ArticleId { get; set; }
        
        [ForeignKey("AuthorId")]
        public virtual User Author { get; set; } = null!;
        
        [ForeignKey("ArticleId")]
        public virtual Article Article { get; set; } = null!;
    }
}
