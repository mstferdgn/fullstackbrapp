using BookResearchApp.Core.Entities.DTOs;
using BookResearchApp.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookResearchApp.Controllers
{
    
    [Route("api/v1/authors")]
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly IAuthorService _authorService;

        public AuthorController(IAuthorService authorService)
        {
            _authorService = authorService;

        }

        /// <summary>
        /// Mevcut tüm yazarları getirir.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllAuthors()
        {
            var authors = await _authorService.GetAllAuthorsAsync();
            return Ok(authors);
        }


        /// <summary>
        /// Belirli bir yazarı ID'ye göre döndürür.
        /// </summary>
        [HttpGet("{authorId:int}")]
        public async Task<IActionResult> GetAuthorById(int authorId)
        {
            var author = await _authorService.GetAuthorByIdAsync(authorId);
            if (author == null)
            {
                return NotFound("Yazar bulunamadı.");
            }

            return Ok(author);
        }

        /// <summary>
        /// Belirli bir yazarın kitaplarını döndürür.
        /// </summary>
        [HttpGet("{authorId}/books")]
        public async Task<IActionResult> GetBooksByAuthor(int authorId)
        {
            var books = await _authorService.GetBooksByAuthorAsync(authorId);
            return Ok(books);
        }

      
        /// <summary>
        /// Yazar ekleme.
        /// </summary>
        [Authorize]
        [HttpPost("create-author")]
        public async Task<IActionResult> AddAuthor([FromBody] AuthorDto authorDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _authorService.AddAuthorAsync(authorDto);
            return Ok("Author added successfully.");
        }

        /// <summary>
        /// Arama: Inputa göre yazar önerilerini getirir.
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchAuthors([FromQuery] string search)
        {
            var authors = await _authorService.SearchAuthorsAsync(search);
            return Ok(authors);
        }

        [HttpGet("review-counts")]
        public async Task<IActionResult> GetAuthorReviewCounts()
        {
            var reviewCounts = await _authorService.GetAuthorReviewCountsAsync();
            return Ok(reviewCounts);
        }

    }

}
