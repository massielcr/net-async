using AutoMapper;
using Books.API.Filters;
using Books.API.Helper;
using Books.API.Models;
using Books.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Books.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(BooksResultFilter))]
    public class BookCollectionsController(IMapper mapper, IBooksRepository booksRepository) : Controller
    {
        private readonly IMapper _mapper = mapper;
        private readonly IBooksRepository _booksRepository = booksRepository;

        [HttpPost]
        public async Task<IActionResult> CreateBookCollections(IEnumerable<BookForCreationDto> bookForCreationDtos)
        {
            var books = _mapper.Map<IEnumerable<Entities.Book>>(bookForCreationDtos);

            foreach (var book in books)
            {
                _booksRepository.AddBook(book);
            }

            await _booksRepository.SaveChangesAsync();

            var booksResponse = await _booksRepository.GetBooksAsync([.. books.Select(b => b.Id)]);
            var bookIds = string.Join(",", booksResponse.Select(b => b.Id));

            return CreatedAtRoute("GetBookCollection", new { ids  = bookIds }, booksResponse);
        }


        [HttpGet("({ids})", Name = "GetBookCollection")]       
        public async Task<IActionResult> GetBookCollections([ModelBinder(BinderType =typeof(ArrayModelBinder))]IEnumerable<Guid> ids)
        {
            var books = await _booksRepository.GetBooksAsync(ids);
            if (ids.Count() != books.Count())
            {
                return NotFound();
            }
            return Ok(books);
        }
    }
}
