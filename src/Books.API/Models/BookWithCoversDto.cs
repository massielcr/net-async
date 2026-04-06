namespace Books.API.Models
{
    public class BookWithCoversDto : BookDto
    {
        public IEnumerable<BookWithCoverDto> BookCovers { get; set; } = [];

        public BookWithCoversDto(Guid id, string authorName, string title, string? description)
            : base(id, authorName, title, description)
        {
        }
    }
}
