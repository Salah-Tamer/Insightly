using System.ComponentModel.DataAnnotations;

namespace Insightly.ViewModels
{
    public class EditProfileViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "Name must be between 1 and 100 characters.")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Profile Picture")]
        public IFormFile? ProfilePictureFile { get; set; }

        public string? ProfilePicture { get; set; }

        [StringLength(500, ErrorMessage = "Bio must be less than 500 characters.")]
        [Display(Name = "Bio")]
        public string? Bio { get; set; }

        public string Email { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
    }
}

