using BookResearchApp.Core.Entities;
using BookResearchApp.Core.Interfaces.Repositories;
using BookResearchApp.Data;
using Microsoft.EntityFrameworkCore;

namespace BookResearchApp.DataAccess.Repository
{
    public class ReviewRepository :BaseRepository<Review,int>, IReviewRepository
    {
        public ReviewRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Review>> GetReviewsByBookIdAsync(int bookId)
        {
            return await _dbSet
                .Where(r => r.BookId == bookId)      ////////////////////////////////////////////////////////////////////////   
                       .Include(r => r.Book)        // Kitap bilgisi                     ///////////////////////////////////                               
                       .Include(r => r.User)       // İncelemeyi yapan kullanıcı        ///////////////////////////////////                               
                       .Include(r => r.Comments)  // Yorumlar                          ///////////////////////////////////                            
                       .ThenInclude(c => c.User) // Yorum yapan kullanıcı bilgisi     ///////////////////////////////////                               
                .ToListAsync();                 ////////////////////////////////////////////////////////////////////////
        }
    }
}
