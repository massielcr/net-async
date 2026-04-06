using AutoMapper;
using Books.API.Entities;
using Books.API.Models;
using Books.API.Models.External;

namespace Books.API.Profiles
{
    public class BooksProfile : Profile
    {
        public BooksProfile() 
        {
            CreateMap<Book, BookDto>()
               .ConstructUsing(src => new BookDto(
                   src.Id,
                   src.Author != null ? $"{src.Author.FirstName} {src.Author.LastName}" : "Unknown Author",
                   src.Title,
                   src.Description
               ));

            CreateMap<BookForCreationDto, Book>()
                .ConstructUsing(src => new Book(
                    Guid.NewGuid(),
                    src.AuthorId,
                    src.Title,
                    src.Description
                ));

            CreateMap<Book, BookWithCoversDto>()
               .ConstructUsing(src => new BookWithCoversDto(
                   src.Id,
                   src.Author != null ? $"{src.Author.FirstName} {src.Author.LastName}" : "Unknown Author",
                   src.Title,
                   src.Description
               ));

            CreateMap<BookCoverDto, BookWithCoverDto>();

            CreateMap<IEnumerable<BookCoverDto>, BookWithCoversDto>()
                .ForMember(dest => dest.BookCovers, opt => opt.MapFrom(src => src));
        }
    }
}
