using BookResearchApp.Core.Entities;
using BookResearchApp.Core.Entities.DTOs;

namespace BookResearchApp.Core.Interfaces.Repositories
{
    public interface ICommentRepository : IBaseRepository<Comment,int>
    {
        Task<IEnumerable<Comment>> GetCommentsByReviewIdAsync(int reviewId);
        Task<PaginationVm<Comment>> GetCommentsByReviewIdPagedAsync(int reviewId, int pageNumber, int pageSize);
    }
}
