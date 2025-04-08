using BookResearchApp.Core.Entities;
using BookResearchApp.Core.Entities.DTOs;
using BookResearchApp.Core.Interfaces.Repositories;
using BookResearchApp.Data;
using Microsoft.EntityFrameworkCore;

namespace BookResearchApp.DataAccess.Repository
{
    public class AuthorRepository : BaseRepository<Author,int>, IAuthorRepository
    {
        public AuthorRepository(AppDbContext context) : base(context) { }

        public async Task<Author> GetAuthorWithBooksAsync(int authorId)
        {
            return await _dbSet
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.Id == authorId);
        }

        public async Task<Author> GetByNameAsync(string name)
        {
            
            return await _dbSet.FirstOrDefaultAsync(a => a.Name.ToUpper() == name);
        }

        public async Task<IEnumerable<Author>> SearchByNameAsync(string search)
        {
            string pattern = search + "%";
            // EF.Functions.ILike, PostgreSQL için case-insensitive arama sağlar.
            return await _dbSet.Where(a => EF.Functions.ILike(a.Name, pattern)).ToListAsync();
        }


        // Yazarların toplam inceleme sayılarını döndürür.
        public async Task<IEnumerable<Author>> GetAuthorsWithReviewsAsync()
        {
            var authors = await _dbSet
                .Include(author => author.Books)
                    .ThenInclude(book => book.Reviews)
                .ToListAsync();

            return authors;
        }
    }
}
