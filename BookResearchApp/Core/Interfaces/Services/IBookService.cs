using BookResearchApp.Core.Entities.Constants;
using BookResearchApp.Core.Entities.DTOs;

namespace BookResearchApp.Core.Interfaces.Services
{
    public interface IBookService
    {
        Task<IEnumerable<BookDto>> GetAllBooksAsync();
        Task<BookDto> GetBookByIdAsync(int id);
        Task<BookDto> AddBookAsync(BookDto bookDto, IFormFile? imageFile, string currentUserName);
        Task<BookDto> UpdateBookAsync(int id, BookDto updatedDto, string currentUserId, IFormFile? imageFile);
        Task<bool> DeleteBookAsync(int id, string currentUserId);
        Task UpdateBookRatingAsync(int bookId);
        Task<IEnumerable<BookDto>> FilterBooksAsync(string? title, BookTypeEnum? type, string? sortRating);
        Task<IEnumerable<BookDto>> GetBooksByUserIdAsync(string userId);
    }
}
