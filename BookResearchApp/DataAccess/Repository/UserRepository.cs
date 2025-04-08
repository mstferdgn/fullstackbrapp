using BookResearchApp.Core.Entities;
using BookResearchApp.Core.Interfaces.Repositories;
using BookResearchApp.Data;
using Microsoft.EntityFrameworkCore;

namespace BookResearchApp.DataAccess.Repository
{
    public class UserRepository : BaseRepository<User, string>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<User> GetByUserNameAsync(string userName)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            // Case-insensitive arama için ToLower kullanabiliriz.
            return await _dbSet.FirstOrDefaultAsync(u => u.EmailAddress.ToLower() == email.ToLower());
        }
    }
}
