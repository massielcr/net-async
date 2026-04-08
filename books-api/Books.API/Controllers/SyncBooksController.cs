using Books.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Books.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SyncBooksController(IBooksRepository booksRepository) : ControllerBase
    {
        [HttpGet]
        public IActionResult GetBooks()
        {
            var books = booksRepository.GetBooks();
            return Ok(books);
        }

        [HttpGet("{id}")]
        public IActionResult GetBook(Guid id)
        {
            var book = booksRepository.GetBook(id);

            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }
    }
}
