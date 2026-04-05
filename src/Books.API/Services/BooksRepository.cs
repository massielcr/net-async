using Books.API.DbContexts;
using Books.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Books.API.Services
{
    public class BooksRepository(BooksDbContext booksDbContext) : IBooksRepository
    {
        private readonly BooksDbContext _booksDbContext = booksDbContext;

        public Book? GetBook(Guid id)
        {
            return _booksDbContext.Books.FirstOrDefault(b => b.Id == id);
        }

        public async Task<Book?> GetBookAsync(Guid id)
        {
            return await _booksDbContext.Books.FirstOrDefaultAsync(b => b.Id == id);
        }

        public IEnumerable<Book> GetBooks()
        {
            return [.. _booksDbContext.Books];
        }

        public async Task<IEnumerable<Book>> GetBooksAsync()
        {
            return await _booksDbContext.Books.ToListAsync();
        }
    }
}
