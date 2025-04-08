using BookResearchApp.Core.Entities;

namespace BookResearchApp.Core.Interfaces.Repositories
{
    public interface IAuthorRepository : IBaseRepository<Author,int>
    {
        Task<Author> GetAuthorWithBooksAsync(int authorId);
        Task<Author> GetByNameAsync(string name);
        Task<IEnumerable<Author>> SearchByNameAsync(string search);
        Task<IEnumerable<Author>> GetAuthorsWithReviewsAsync();
    }
}
