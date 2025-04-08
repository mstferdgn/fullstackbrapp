using BookResearchApp.Core.Entities;
using BookResearchApp.Core.Entities.Constants;

namespace BookResearchApp.Core.Interfaces.Repositories
{
    public interface IBookRepository :IBaseRepository<Book,int>
    {
        Task<Book> GetBookWithReviewsAsync(int bookId);
        Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId);
        Task<Book> GetByTitleAsync(string title);
        Task<IEnumerable<Book>> FilterBooksAsync(string? title, BookTypeEnum? type, string? sortRating);
        Task<IEnumerable<Book>> GetBooksByUserIdAsync(string userId);
    }
}
