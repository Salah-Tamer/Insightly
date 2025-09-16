using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Insightly.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        [StringLength(255)]
        public string? ProfilePicture { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual List<Article> Articles { get; set; } = new();
        public virtual List<ArticleRead> SavedArticles { get; set; } = new();
        public virtual List<Comment> Comments { get; set; } = new(); 
        public virtual List<Vote> Votes { get; set; } = new();
        public virtual List<CommentVote> CommentVotes { get; set; } = new();
        public virtual List<Follow> Followers { get; set; } = new(); 
        public virtual List<Follow> Following { get; set; } = new();
    }
}



