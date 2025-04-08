using System.Linq.Expressions;

namespace BookResearchApp.Core.Interfaces.Repositories
{
    public interface IBaseRepository<T,TKey> where T : class
    {
        Task<T?> GetByIdAsync(TKey id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        //Task<int> SaveChangesAsync();
    }
}
