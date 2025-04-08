using BookResearchApp.Core.Entities;

namespace BookResearchApp.Core.Interfaces.Repositories
{
    public interface IReviewRepository : IBaseRepository<Review,int>
    {
        Task<IEnumerable<Review>> GetReviewsByBookIdAsync(int bookId);

    }
}
