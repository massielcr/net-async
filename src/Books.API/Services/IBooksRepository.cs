using Books.API.Entities;

namespace Books.API.Services
{
    public interface IBooksRepository
    {
        IEnumerable<Book> GetBooks();
        Book? GetBook(Guid id);

        Task<IEnumerable<Book>> GetBooksAsync();
        Task<Book?> GetBookAsync(Guid id);
        
    }
}
