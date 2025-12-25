using AutoMapper;
using Insightly.Models;
using Insightly.ViewModels;

namespace Insightly.Mappings
{
    public class ArticleMappingProfile : Profile
    {
        public ArticleMappingProfile()
        {
            // Article to ArticleDetailsViewModel
            CreateMap<Article, ArticleDetailsViewModel>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.Name : string.Empty))
                .ForMember(dest => dest.AuthorProfilePicture, opt => opt.MapFrom(src => src.Author != null ? src.Author.ProfilePicture : null))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments != null ? src.Comments : new List<Comment>()));

            // Article to ArticleListItemViewModel
            CreateMap<Article, ArticleListItemViewModel>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.Name : string.Empty))
                .ForMember(dest => dest.AuthorProfilePicture, opt => opt.MapFrom(src => src.Author != null ? src.Author.ProfilePicture : null));

            // Article to ArticleEditViewModel
            CreateMap<Article, ArticleEditViewModel>();

            // Comment to CommentViewModel
            CreateMap<Comment, CommentViewModel>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.Name : string.Empty))
                .ForMember(dest => dest.AuthorProfilePicture, opt => opt.MapFrom(src => src.Author != null ? src.Author.ProfilePicture : null));

            // Article to ArticleJsonDto (for JSON responses)
            CreateMap<Article, ArticleJsonDto>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => new AuthorJsonDto
                {
                    Name = src.Author != null ? src.Author.Name : string.Empty,
                    Id = src.AuthorId
                }));

            // Comment to CommentJsonDto (for JSON responses)
            CreateMap<Comment, CommentJsonDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CommentId))
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author != null ? src.Author.Name : string.Empty))
                .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.AuthorId))
                .ForMember(dest => dest.AuthorProfilePicture, opt => opt.MapFrom(src => src.Author != null ? src.Author.ProfilePicture : null))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("dd MMM yyyy HH:mm")))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.HasValue ? src.UpdatedAt.Value.ToString("dd MMM yyyy HH:mm") : null))
                .ForMember(dest => dest.IsUpdated, opt => opt.MapFrom(src => src.UpdatedAt.HasValue));
        }
    }
}

