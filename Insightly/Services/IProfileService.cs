using Insightly.ViewModels;

namespace Insightly.Services
{
    public interface IProfileService
    {
        Task<(bool Success, string? ErrorMessage)> UpdateProfileAsync(string userId, EditProfileViewModel model);
        Task<ProfileViewModel?> GetProfileViewModelAsync(string userId);

        Task<MyProfilePageViewModel?> GetMyProfilePageAsync(string userId);

        Task<PublicProfileRequestResult> GetPublicProfilePageAsync(string targetUserId, string? viewerUserId);

        Task<EditProfileViewModel?> GetEditProfileAsync(string userId);
    }
}
