using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Insightly.Models
{
    public class Vote
    {
        public int VoteId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int ArticleId { get; set; }

        [Required]
        public bool IsUpvote { get; set; } 

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("ArticleId")]
        public virtual Article Article { get; set; } = null!;
    }

}
