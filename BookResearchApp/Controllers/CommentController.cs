using BookResearchApp.Core.Entities.DTOs;
using BookResearchApp.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookResearchApp.Controllers
{
    
    [Route("api/v1/comments")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("{reviewId:int}")]
        public async Task<IActionResult> GetCommentsByReview(int reviewId)
        {
            var comments = await _commentService.GetCommentsByReviewIdAsync(reviewId);
            return Ok(comments);
        }

        [HttpGet("comment-list/{reviewId:int}")]
        public async Task<IActionResult> GetCommentsByReviewPaged(int reviewId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var pagedComments = await _commentService.GetCommentsByReviewIdPagedAsync(reviewId, pageNumber, pageSize);

            return Ok(pagedComments);
        }
        [Authorize]
        [HttpPost("create-comment")]
        public async Task<IActionResult> AddComment([FromBody] CommentDto commentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string currentUserName = User.FindFirst(ClaimTypes.Name)?.Value;

            try
            {
                await _commentService.AddCommentAsync(commentDto, currentUserId, currentUserName);
                return Ok("Yorum başarıyla eklendi.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPut("edit-comment/{commentId:int}")]
        public async Task<IActionResult> UpdateComment(int commentId, [FromBody] CommentDto commentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string currentUserName = User.FindFirst(ClaimTypes.Name)?.Value;

            commentDto.Id = commentId;
            try
            {
                await _commentService.UpdateCommentAsync(commentDto, currentUserId, currentUserName);
                return Ok("Yorum başarıyla güncellendi.");
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize]
        [HttpDelete("delete-comment/{commentId:int}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            
            string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                await _commentService.DeleteCommentAsync(commentId, currentUserId);
                return Ok("Yorum başarıyla silindi.");
            }
    
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            }

    }
}
