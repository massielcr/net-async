using AutoMapper;
using Books.API.Models;
using Books.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Books.API.Controllers
{
    [Route("api/bookcollections")]
    [ApiController]
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

            return Ok();
        }
    }
}
