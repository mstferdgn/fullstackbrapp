using BookResearchApp.Core.Entities.DTOs;

namespace BookResearchApp.Core.Interfaces.Services
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetReviewsByBookIdAsync(int bookId);
        Task AddReviewAsync(ReviewDto reviewDto, string currentUserId, string currentUserName);
        Task UpdateReviewAsync(ReviewDto reviewDto, string currentUserId, string currentUserName);
        Task DeleteReviewAsync(int reviewId, string currentUserId);

    }
}
