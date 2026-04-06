using Books.API.Entities;

namespace Books.API.Services
{
    public interface IBooksRepository
    {
        void AddBook(Book book);
        void DeleteBook(Book book);

        Book? GetBook(Guid id);

        IEnumerable<Book> GetBooks();
        

        Task<IEnumerable<Book>> GetBooksAsync();        

        Task<IEnumerable<Book>> GetBooksAsync(IEnumerable<Guid> ids);

        IAsyncEnumerable<Book> GetBooksAsAsyncEnumerable();

        Task<Book?> GetBookAsync(Guid id);  
        
        Task<bool> SaveChangesAsync();
    }
}
