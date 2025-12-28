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
        private readonly IFileUploadService _fileUploadService;
        private readonly IMapper _mapper;

        public ProfileService(
            UserManager<ApplicationUser> userManager, 
            IArticleRepository articleRepository,
            IFollowRepository followRepository,
            IFileUploadService fileUploadService, 
            IMapper mapper)
        {
            _userManager = userManager;
            _articleRepository = articleRepository;
            _followRepository = followRepository;
            _fileUploadService = fileUploadService;
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

            if (model.ProfilePictureFile != null && model.ProfilePictureFile.Length > 0)
            {
                var (isValid, validationError) = await _fileUploadService.ValidateImageAsync(model.ProfilePictureFile);
                if (!isValid)
                {
                    return (false, validationError ?? "Invalid image file.");
                }

                var profilePicturePath = await _fileUploadService.UploadProfilePictureAsync(model.ProfilePictureFile, userId);
                if (profilePicturePath != null)
                {
                    user.ProfilePicture = profilePicturePath;
                }
            }

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

        public async Task<(ProfileViewModel? Profile, List<ArticleListItemViewModel> Articles, int FollowersCount, int FollowingCount)> GetProfileWithDetailsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (null, new List<ArticleListItemViewModel>(), 0, 0);
            }

            var articles = await _articleRepository.GetByAuthorIdAsync(userId);
            var followersCount = await _followRepository.GetFollowersCountAsync(userId);
            var followingCount = await _followRepository.GetFollowingCountAsync(userId);

            var articleViewModels = _mapper.Map<List<ArticleListItemViewModel>>(articles);
            var profileViewModel = _mapper.Map<ProfileViewModel>(user);

            return (profileViewModel, articleViewModels, followersCount, followingCount);
        }

        public async Task<(ProfileViewModel? Profile, List<ArticleListItemViewModel> Articles, int FollowersCount, int FollowingCount, bool IsFollowing)> GetProfileWithDetailsAsync(string userId, string? currentUserId)
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
            if (!string.IsNullOrEmpty(currentUserId))
            {
                isFollowing = await _followRepository.ExistsAsync(currentUserId, userId);
            }

            var articleViewModels = _mapper.Map<List<ArticleListItemViewModel>>(articles);
            var profileViewModel = _mapper.Map<ProfileViewModel>(user);

            return (profileViewModel, articleViewModels, followersCount, followingCount, isFollowing);
        }
    }
}

