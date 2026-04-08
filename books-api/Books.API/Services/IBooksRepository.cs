using Books.API.Entities;
using Books.API.Models.External;

namespace Books.API.Services
{
    public interface IBooksRepository
    {
        void AddBook(Book book);

        void DeleteBook(Book book);

        Book? GetBook(Guid id);

        Task<Book?> GetBookAsync(Guid id);

        IEnumerable<Book> GetBooks();       

        Task<IEnumerable<Book>> GetBooksAsync();        

        Task<IEnumerable<Book>> GetBooksAsync(IEnumerable<Guid> ids);

        IAsyncEnumerable<Book> GetBooksAsAsyncEnumerable();

                 

        Task<bool> SaveChangesAsync();


        #region Book Cover

        Task<BookCoverDto?> GetBookCoverAsync(string id);

        Task<IEnumerable<BookCoverDto>> GetBookCoversProcessOneByOneAsync(Guid bookId, CancellationToken cancellationToken);

        Task<IEnumerable<BookCoverDto>> GetBookCoversProcessAfterWaitForAllAsync(Guid bookId);

        #endregion
    }
}
