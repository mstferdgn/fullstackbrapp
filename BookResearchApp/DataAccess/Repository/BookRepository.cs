using BookResearchApp.Core.Entities;
using BookResearchApp.Core.Entities.Constants;
using BookResearchApp.Core.Interfaces.Repositories;
using BookResearchApp.Data;
using Microsoft.EntityFrameworkCore;

namespace BookResearchApp.DataAccess.Repository
{
    public class BookRepository :BaseRepository<Book,int>, IBookRepository
    {
        public BookRepository(AppDbContext context) : base(context) { }

        public async Task<Book> GetBookWithReviewsAsync(int bookId)
        {
            return await _dbSet
                .Include(b => b.Reviews)
                    .ThenInclude(r => r.Comments)
                .FirstOrDefaultAsync(b => b.Id == bookId);
        }

        public async Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId)
        {
            return await _dbSet
            .Where(b => b.AuthorId == authorId)
            .ToListAsync();
        }
        public async Task<Book> GetByTitleAsync(string title)
        {
            
            return await _dbSet.FirstOrDefaultAsync(b => b.Title.ToUpper() == title);
        }


        //Kitap filtreleme işlemi için
        public async Task<IEnumerable<Book>> FilterBooksAsync(string? title, BookTypeEnum? type, string? sortRating)
        {
            var query = _dbSet.AsQueryable();

            // Başlık araması: EF.Functions.ILike ile case-insensitive
            if (!string.IsNullOrWhiteSpace(title))
            {
                string pattern = title + "%";
                query = query.Where(b => EF.Functions.ILike(b.Title, pattern));
            }

            // Kitap türü filtresi
            if (type.HasValue)
            {
                query = query.Where(b => b.Type == type.Value);
            }

            // Rating sıralaması
            if (!string.IsNullOrWhiteSpace(sortRating))
            {
                if (sortRating.ToLowerInvariant() == "asc")
                    query = query.OrderBy(b => b.Rating);
                else if (sortRating.ToLowerInvariant() == "desc")
                    query = query.OrderByDescending(b => b.Rating);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooksByUserIdAsync(string userId)
        {
            return await _dbSet
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }
    }
}
