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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual List<Article> Articles { get; set; } = new();
        public virtual List<ArticleRead> ReadArticles { get; set; } = new();
        public virtual List<Comment> Comments { get; set; } = new(); 
        public virtual List<Vote> Votes { get; set; } = new();
    }
}
