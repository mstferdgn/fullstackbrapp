using BookResearchApp.Core.Entities.DTOs;
using BookResearchApp.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookResearchApp.Controllers
{

    [Route("api/v1/reviews")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Belirli bir kitaba ait tüm incelemeleri getirir.
        /// </summary>
        [HttpGet("{bookId:int}")]
        public async Task<IActionResult> GetReviewsByBook(int bookId)
        {
            var reviews = await _reviewService.GetReviewsByBookIdAsync(bookId);
            if (reviews == null)
            {
                return NotFound("Kitaba ait inceleme bulunamadı.");
            }

            return Ok(reviews);
        }


        /// <summary>
        /// Yeni bir kitap incelemesi oluşturur.
        /// </summary>
        [Authorize]
        [HttpPost("create-review")]
        public async Task<IActionResult> AddReview([FromBody] ReviewDto reviewDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // JWT token'dan kullanıcı bilgilerini al
            string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string currentUserName = User.FindFirst(ClaimTypes.Name)?.Value;
            try
            {
                await _reviewService.AddReviewAsync(reviewDto, currentUserId, currentUserName);
                return Ok("Review başarıyla eklendi.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPut("update-review/{reviewId:int}")]
        public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] ReviewDto reviewDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            // JWT token'dan kullanıcı bilgilerini al
            string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string currentUserName = User.FindFirst(ClaimTypes.Name)?.Value;


            reviewDto.Id = reviewId;
            try
            {
                await _reviewService.UpdateReviewAsync(reviewDto, currentUserId, currentUserName);
                return Ok("Review başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpDelete("delete-review/{reviewId:int}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                await _reviewService.DeleteReviewAsync(reviewId, currentUserId);
                return Ok("Review başarıyla silindi.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}