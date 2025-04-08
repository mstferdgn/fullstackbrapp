using BookResearchApp.Core.Entities;
using BookResearchApp.Core.Entities.DTOs;
using BookResearchApp.Core.Interfaces.Repositories;
using BookResearchApp.Data;
using Microsoft.EntityFrameworkCore;

namespace BookResearchApp.DataAccess.Repository
{
    public class CommentRepository : BaseRepository<Comment,int>, ICommentRepository
    {
        public CommentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Comment>> GetCommentsByReviewIdAsync(int reviewId)
        {
            return await _dbSet.Where(c => c.ReviewId == reviewId).ToListAsync();
        }


        public async Task<PaginationVm<Comment>> GetCommentsByReviewIdPagedAsync(int reviewId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _dbSet.Where(c => c.ReviewId == reviewId)
                .Include(c => c.User);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(c => c.CreatedAt) // Azalan sıralama (En yeni yorumlar önce)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

                return new PaginationVm<Comment>()
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
        }
    }
}
