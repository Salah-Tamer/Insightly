using AutoMapper;
using Insightly.Models;
using Insightly.Repositories;
using Insightly.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace Insightly.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IArticleRepository _articleRepository;
        private readonly IFollowRepository _followRepository;
        private readonly IMapper _mapper;

        public ProfileService(
            UserManager<ApplicationUser> userManager,
            IArticleRepository articleRepository,
            IFollowRepository followRepository,
            IMapper mapper)
        {
            _userManager = userManager;
            _articleRepository = articleRepository;
            _followRepository = followRepository;
            _mapper = mapper;
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateProfileAsync(string userId, EditProfileViewModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            user.Name = model.Name;
            user.Bio = model.Bio;

            // TODO: Implement profile picture (pfp) upload logic

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, $"Failed to update profile: {errors}");
            }

            return (true, null);
        }

        public async Task<ProfileViewModel?> GetProfileViewModelAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            return _mapper.Map<ProfileViewModel>(user);
        }

        public async Task<MyProfilePageViewModel?> GetMyProfilePageAsync(string userId)
        {
            var details = await LoadProfileDetailsAsync(userId, viewerUserId: null);
            if (details.Profile == null)
            {
                return null;
            }

            return new MyProfilePageViewModel
            {
                Profile = details.Profile,
                Articles = details.Articles,
                FollowersCount = details.FollowersCount,
                FollowingCount = details.FollowingCount
            };
        }

        public async Task<PublicProfileRequestResult> GetPublicProfilePageAsync(string targetUserId, string? viewerUserId)
        {
            if (!string.IsNullOrEmpty(viewerUserId) && viewerUserId == targetUserId)
            {
                return new PublicProfileRequestResult { ViewerIsOwner = true };
            }

            var details = await LoadProfileDetailsAsync(targetUserId, viewerUserId);
            if (details.Profile == null)
            {
                return new PublicProfileRequestResult { UserNotFound = true };
            }

            var page = new PublicProfilePageViewModel
            {
                Profile = details.Profile,
                Articles = details.Articles,
                FollowersCount = details.FollowersCount,
                FollowingCount = details.FollowingCount,
                IsFollowing = details.IsFollowing,
                ShowFollowButton = !string.IsNullOrEmpty(viewerUserId) && viewerUserId != details.Profile.Id
            };

            return new PublicProfileRequestResult { Page = page };
        }

        public async Task<EditProfileViewModel?> GetEditProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            return new EditProfileViewModel
            {
                Name = user.Name,
                ProfilePicture = user.ProfilePicture,
                Bio = user.Bio,
                Email = user.Email ?? string.Empty,
                Gender = user.Gender
            };
        }

        private async Task<(ProfileViewModel? Profile, List<ArticleListItemViewModel> Articles, int FollowersCount, int FollowingCount, bool IsFollowing)> LoadProfileDetailsAsync(string userId, string? viewerUserId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (null, new List<ArticleListItemViewModel>(), 0, 0, false);
            }

            var articles = await _articleRepository.GetByAuthorIdAsync(userId);
            var followersCount = await _followRepository.GetFollowersCountAsync(userId);
            var followingCount = await _followRepository.GetFollowingCountAsync(userId);

            var isFollowing = false;
            if (!string.IsNullOrEmpty(viewerUserId))
            {
                isFollowing = await _followRepository.ExistsAsync(viewerUserId, userId);
            }

            var articleViewModels = _mapper.Map<List<ArticleListItemViewModel>>(articles);
            var profileViewModel = _mapper.Map<ProfileViewModel>(user);

            return (profileViewModel, articleViewModels, followersCount, followingCount, isFollowing);
        }
    }
}
