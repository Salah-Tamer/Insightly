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
        public string UserId { get; set; } = string.Empty;  

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
