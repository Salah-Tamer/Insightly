using AutoMapper;
using Insightly.Models;
using Insightly.ViewModels;

namespace Insightly.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            // ApplicationUser to ProfileViewModel
            CreateMap<ApplicationUser, ProfileViewModel>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email ?? string.Empty));

            // ApplicationUser to UserListItemViewModel
            CreateMap<ApplicationUser, UserListItemViewModel>();

            // Chat to ChatViewModel
            CreateMap<Chat, ChatViewModel>()
                .ForMember(dest => dest.OtherUserName, opt => opt.MapFrom(src => 
                    src.OtherUser != null ? src.OtherUser.Name : string.Empty))
                .ForMember(dest => dest.OtherUserProfilePicture, opt => opt.MapFrom(src => 
                    src.OtherUser != null ? src.OtherUser.ProfilePicture : null))
                .ForMember(dest => dest.Messages, opt => opt.MapFrom(src => 
                    src.Messages != null ? src.Messages : new List<ChatMessage>()));

            // ChatMessage to MessageViewModel
            CreateMap<ChatMessage, MessageViewModel>();
        }
    }
}

