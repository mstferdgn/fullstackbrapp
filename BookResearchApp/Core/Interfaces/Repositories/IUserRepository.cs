using BookResearchApp.Core.Entities;

namespace BookResearchApp.Core.Interfaces.Repositories
{
    public interface IUserRepository : IBaseRepository<User,string>
    {
        Task<User> GetByUserNameAsync(string userName);
        Task<User> GetByEmailAsync(string email);
    }
}
