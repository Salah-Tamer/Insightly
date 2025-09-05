using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Insightly.Models
{
    public enum ReactionType
    {
        None = 0,
        Like = 1,
        Dislike = 2
    }

    public class Reaction
    {
        public int ReactionId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int ArticleId { get; set; }

        public ReactionType Type { get; set; } = ReactionType.None;
      
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("ArticleId")]
        public virtual Article Article { get; set; } = null!;
    }

}
