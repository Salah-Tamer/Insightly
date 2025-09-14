using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Insightly.Models
{
    public class CommentVote
    {
        public int CommentVoteId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int CommentId { get; set; }

        public bool IsUpvote { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("CommentId")]
        public virtual Comment Comment { get; set; } = null!;
    }
}


