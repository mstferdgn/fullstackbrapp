using BookResearchApp.Core.Entities.DTOs;

namespace BookResearchApp.Core.Interfaces.Services
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentDto>> GetCommentsByReviewIdAsync(int reviewId);
        Task<PaginationVm<CommentDto>> GetCommentsByReviewIdPagedAsync(int reviewId, int pageNumber, int pageSize);
        Task AddCommentAsync(CommentDto commentDto, string currentUserId, string currentUserName);
        Task UpdateCommentAsync(CommentDto commentDto, string currentUserId, string currentUserName);
        Task DeleteCommentAsync(int commentId, string currentUserId);
    }
}
