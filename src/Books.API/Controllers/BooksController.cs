using Books.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Books.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController(IBooksRepository booksRepository) : ControllerBase
    {
        private readonly IBooksRepository _booksRepository = booksRepository;

        [HttpGet]
        public async Task<IActionResult> GetBooks()
        {
            var books = await _booksRepository.GetBooksAsync();

            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBook(Guid id)
        {
            var book = await _booksRepository.GetBookAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }
    }
}
