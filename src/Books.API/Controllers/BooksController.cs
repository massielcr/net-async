using AutoMapper;
using Books.API.Entities;
using Books.API.Filters;
using Books.API.Models;
using Books.API.Models.External;
using Books.API.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Books.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController(IMapper mapper, IBooksRepository booksRepository) : ControllerBase
    {
        private readonly IMapper _mapper = mapper;
        private readonly IBooksRepository _booksRepository = booksRepository;

        [HttpGet]
        [TypeFilter(typeof(BooksResultFilter))]
        public async Task<IActionResult> GetBooks()
        {
            var books = await _booksRepository.GetBooksAsync();

            return Ok(books);
        }

        [HttpGet("bookstream")]
        public async IAsyncEnumerable<BookDto> StreamBooks()
        {
            await foreach (var book in _booksRepository.GetBooksAsAsyncEnumerable())
            {
                yield return _mapper.Map<BookDto>(book); ;
            }
        }

        [HttpGet("{id}", Name ="GetBook")]
        [TypeFilter(typeof(BookWithCoverResultFilter))]
        public async Task<IActionResult> GetBook(Guid id)
        {
            var book = await _booksRepository.GetBookAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            //var bookCover = await _booksRepository.GetBookCoverAsync("dummycover");

            //var bookCovers = await _booksRepository.GetBookCoversProcessOneByOneAsync(id);

            var bookCovers = await _booksRepository.GetBookCoversProcessAfterWaitForAllAsync(id);

            (Book book, IEnumerable<BookCoverDto> bookCovers) propertyBag = new (book, bookCovers);

            return Ok((book, bookCovers));
        }

        [HttpPost]
        [TypeFilter(typeof(BookResultFilter))]
        public async Task<IActionResult> CreateBook([FromBody] BookForCreationDto bookForCreationDto)
        {
            var book = _mapper.Map<Book>(bookForCreationDto);

            _booksRepository.AddBook(book);

            await _booksRepository.SaveChangesAsync();
            await _booksRepository.GetBookAsync(book.Id);

            return CreatedAtRoute("GetBook", new { id = book.Id }, book);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBook(Guid id)
        {
            var book = await _booksRepository.GetBookAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            _booksRepository.DeleteBook(book);

            await _booksRepository.SaveChangesAsync();

            return NoContent();
        }
    }
}
