using Books.API.DbContexts;
using Books.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Books.API.Services
{
    public class BooksRepository(BooksDbContext booksDbContext) : IBooksRepository
    {
        private readonly BooksDbContext _booksDbContext = booksDbContext;

        public void AddBook(Book book)
        {
            ArgumentNullException.ThrowIfNull(book);

            _booksDbContext.Books.Add(book);
        }

        public void DeleteBook(Book book)
        {
            _booksDbContext.Books.Remove(book);
        }

        public Book? GetBook(Guid id)
        {
            return _booksDbContext.Books.Include(b => b.Author).FirstOrDefault(b => b.Id == id);
        }

        public IEnumerable<Book> GetBooks()
        {
            return [.. _booksDbContext.Books.Include(b => b.Author)];
        }

        public async Task<Book?> GetBookAsync(Guid id)
        {
            return await _booksDbContext.Books.Include(b => b.Author).FirstOrDefaultAsync(b => b.Id == id);
        }        

        public async Task<IEnumerable<Book>> GetBooksAsync()
        {
            return await _booksDbContext.Books.Include(b => b.Author).ToListAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _booksDbContext.SaveChangesAsync() > 0;
        }        
    }
}
