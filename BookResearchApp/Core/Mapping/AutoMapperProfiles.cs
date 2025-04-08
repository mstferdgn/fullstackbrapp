using AutoMapper;
using BookResearchApp.Core.Entities.DTOs;
using BookResearchApp.Core.Entities;
using BookResearchApp.Core.Entities.Constants;

namespace BookResearchApp.Core.Mapping
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Author mappings
            // Author mapping: Basitçe, DTO'daki Name değeri doğrudan entity'ye aktarılır.
            CreateMap<AuthorDto, Author>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
            CreateMap<Author, AuthorDto>().ReverseMap();

       
            CreateMap<Book, BookDto>()
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
               .ReverseMap()
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<BookTypeEnum>(src.Type)))
               .ReverseMap();


            // Review mappings
            CreateMap<Review, ReviewDto>()
               // BookTitle, Review nesnesinin Book navigation property'sindeki Title'dan alınır
               .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book.Title))
               // Diğer alanlar otomatik maplenecek
               .ReverseMap()
               // Ters dönüşümde, navigation property'yi otomatik maplememek için ignore ettik
               .ForMember(dest => dest.User, opt => opt.Ignore())
               .ForMember(dest => dest.Book, opt => opt.Ignore());


            // Comment mappings
            CreateMap<Comment, CommentDto>()
             .ReverseMap();


            // User mappings 
            CreateMap<User, UserDto>().ReverseMap();
        }
    }
}
