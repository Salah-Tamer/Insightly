using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Insightly.Models
{
    public class ArticleRead
    {
        public int Id { get; set; }
        public DateTime ReadAt { get; set; } = DateTime.Now;
        
        [Required]
        public int ArticleId { get; set; }
        
        [ForeignKey("ArticleId")]
        public virtual Article Article { get; set; } = null!;

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
