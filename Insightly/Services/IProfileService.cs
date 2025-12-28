using Insightly.Models;
using Insightly.ViewModels;

namespace Insightly.Services
{
    public interface IProfileService
    {
        Task<(bool Success, string? ErrorMessage)> UpdateProfileAsync(string userId, EditProfileViewModel model);
        Task<ProfileViewModel?> GetProfileViewModelAsync(string userId);
        Task<(ProfileViewModel? Profile, List<ArticleListItemViewModel> Articles, int FollowersCount, int FollowingCount)> GetProfileWithDetailsAsync(string userId);
        Task<(ProfileViewModel? Profile, List<ArticleListItemViewModel> Articles, int FollowersCount, int FollowingCount, bool IsFollowing)> GetProfileWithDetailsAsync(string userId, string? currentUserId);
    }
}


