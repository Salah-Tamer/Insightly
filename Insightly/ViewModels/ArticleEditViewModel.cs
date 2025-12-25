using System.ComponentModel.DataAnnotations;

namespace Insightly.ViewModels
{
    public class ArticleEditViewModel
    {
        public int ArticleId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;
    }
}

