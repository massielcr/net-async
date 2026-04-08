using Books.API.DbContexts;
using Books.API.Entities;
using Books.API.Models.External;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Books.API.Services
{
    public class BooksRepository(BooksDbContext booksDbContext, IHttpClientFactory httpClientFactory) : IBooksRepository
    {
        private const string bookCoversBaseAddess = "https://localhost:7242";
        
        private readonly BooksDbContext _booksDbContext = booksDbContext;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

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

        public async Task<Book?> GetBookAsync(Guid id)
        {
            return await _booksDbContext.Books.Include(b => b.Author).FirstOrDefaultAsync(b => b.Id == id);
        }

        public IEnumerable<Book> GetBooks()
        {
            return [.. _booksDbContext.Books.Include(b => b.Author)];
        }               

        public async Task<IEnumerable<Book>> GetBooksAsync()
        {
            return await _booksDbContext.Books.Include(b => b.Author).ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooksAsync(IEnumerable<Guid> ids)
        {
            return await _booksDbContext.Books
                            .Where(b => ids.Contains(b.Id))
                            .Include(b => b.Author)
                            .ToListAsync();
        }

        public IAsyncEnumerable<Book> GetBooksAsAsyncEnumerable()
        {
            return _booksDbContext.Books.Include(b => b.Author).AsAsyncEnumerable();
        }        

        public async Task<bool> SaveChangesAsync()
        {
            return await _booksDbContext.SaveChangesAsync() > 0;
        }

        #region Book Cover

        public async Task<BookCoverDto?> GetBookCoverAsync(string id)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.GetAsync($"{bookCoversBaseAddess}/api/bookcovers/{id}");

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<BookCoverDto>
                                (
                                    await response.Content.ReadAsStringAsync(),
                                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                                );
            }

            return null;
        }

        public async Task<IEnumerable<BookCoverDto>> GetBookCoversProcessOneByOneAsync(Guid bookId, CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var bookCovers = new List<BookCoverDto>();

            var bookCoversUrl = new[]
            {
                $"{bookCoversBaseAddess}/api/bookcovers/{bookId}-dummycover1",
                $"{bookCoversBaseAddess}/api/bookcovers/{bookId}-dummycover2",
                $"{bookCoversBaseAddess}/api/bookcovers/{bookId}-dummycover3",
                $"{bookCoversBaseAddess}/api/bookcovers/{bookId}-dummycover4",
                $"{bookCoversBaseAddess}/api/bookcovers/{bookId}-dummycover5"
            };

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                using (var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationTokenSource.Token))
                {
                    foreach (var url in bookCoversUrl)
                    {
                        var response = await httpClient.GetAsync(url, linkedCancellationTokenSource.Token);
                        if (response.IsSuccessStatusCode)
                        {
                            var bookCover = JsonSerializer.Deserialize<BookCoverDto>
                                (
                                    await response.Content.ReadAsStringAsync(linkedCancellationTokenSource.Token),
                                    new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }
                                );
                            if (bookCover != null)
                            {
                                bookCovers.Add(bookCover);
                            }
                        }
                        else
                        {
                            cancellationTokenSource.Cancel();
                        }
                    }
                }
            }

            return bookCovers;
        }

        public async Task<IEnumerable<BookCoverDto>> GetBookCoversProcessAfterWaitForAllAsync(Guid bookId)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var bookCovers = new List<BookCoverDto>();

            var bookCoversUrl = new[]
            {
                $"{bookCoversBaseAddess}/api/bookcovers/{bookId}-dummycover1",
                $"{bookCoversBaseAddess}/api/bookcovers/{bookId}-dummycover2",
                $"{bookCoversBaseAddess}/api/bookcovers/{bookId}-dummycover3",
                $"{bookCoversBaseAddess}/api/bookcovers/{bookId}-dummycover4",
                $"{bookCoversBaseAddess}/api/bookcovers/{bookId}-dummycover5"
            };

            var bookCoverTasks = new List<Task<HttpResponseMessage>>();
            foreach (var url in bookCoversUrl)
            {
                bookCoverTasks.Add(httpClient.GetAsync(url));
            }

            var bookCoverTasksResults = await Task.WhenAll(bookCoverTasks);
            foreach (var bookCover in bookCoverTasksResults)
            {
                if (bookCover.IsSuccessStatusCode)
                {
                    var bookCoverDto = JsonSerializer.Deserialize<BookCoverDto>
                        (
                            await bookCover.Content.ReadAsStringAsync(),
                            new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }
                        );
                    if (bookCoverDto != null)
                    {
                        bookCovers.Add(bookCoverDto);
                    }
                }
            }

            return bookCovers;
        }

        #endregion
    }
}
