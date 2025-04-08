using BookResearchApp.Core.Entities.Constants;
using BookResearchApp.Core.Entities.DTOs;
using BookResearchApp.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookResearchApp.Controllers
{
    
    [Route("api/v1/books")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;

        }

        /// <summary>
        /// Tüm kitapları getirir.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBooks()
        {
            var books = await _bookService.GetAllBooksAsync();

            if (books == null || !books.Any())
                return NotFound("Kayıtlı kitap bulunamadı.");

            return Ok(books);
        }

        /// <summary>
        /// Belirli bir kitabı getirir.
        /// </summary>
        [HttpGet("select-book/{id:int}")]
        public async Task<IActionResult> GetBook(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
               return NotFound("Kitap bulunamadı.");
            }
            return Ok(book);
        }

        /// <summary>AA
        /// Yeni bir kitap oluşturur.
        /// </summary>
        [Authorize]
        [HttpPost("create-book")]
        public async Task<IActionResult> AddBook([FromForm] BookDto bookDto, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string currentUserName = User.FindFirst(ClaimTypes.Name)?.Value;

            try
            {
                var createdBook = await _bookService.AddBookAsync(bookDto, imageFile, currentUserName);
                return Ok(createdBook);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("book-filter")]
        public async Task<IActionResult> FilterBooks([FromQuery] string? title, [FromQuery] BookTypeEnum? type, [FromQuery] string? sortRating)
        {
            var books = await _bookService.FilterBooksAsync(title, type, sortRating);
            return Ok(books);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateBook(int id, [FromForm] BookDto updatedDto, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // JWT token'dan kullanıcı ID'sini al
            string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("Kullanıcı kimliği alınamadı.");

            try
            {
                var updatedBook = await _bookService.UpdateBookAsync(id, updatedDto, currentUserId, imageFile);
                return Ok(updatedBook);
            }
           
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            // JWT token'dan kullanıcı ID'sini al
            string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                bool result = await _bookService.DeleteBookAsync(id, currentUserId);
                if (result)
                    return Ok("Kitap başarıyla silindi.");
                else
                    return NotFound("Kitap bulunamadı.");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize]
        [HttpGet("my-books")]
        public async Task<IActionResult> GetMyBooks()
        {
            string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var books = await _bookService.GetBooksByUserIdAsync(currentUserId);
            if (books == null || !books.Any())
                return NotFound("Kullanıcının oluşturduğu kitap bulunamadı.");

            return Ok(books);
        }

    }
}