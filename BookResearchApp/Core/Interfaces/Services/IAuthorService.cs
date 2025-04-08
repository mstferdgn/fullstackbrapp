using BookResearchApp.Core.Entities.DTOs;

namespace BookResearchApp.Core.Interfaces.Services
{
    public interface IAuthorService
    {
        Task<AuthorDto> GetAuthorByIdAsync(int authorId);
        Task<IEnumerable<BookDto>> GetBooksByAuthorAsync(int authorId);
        Task AddAuthorAsync(AuthorDto authorDto);
        Task<IEnumerable<AuthorDto>> SearchAuthorsAsync(string search);
        Task<IEnumerable<AuthorDto>> GetAllAuthorsAsync();
        Task<IEnumerable<AuthorReviewCountDto>> GetAuthorReviewCountsAsync();
    }
}
