using BookResearchApp.Core.Interfaces.Repositories;
using BookResearchApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookResearchApp.DataAccess.Repository
{
    public class BaseRepository<T, TKey> : IBaseRepository<T, TKey> where T : class
    {

        private readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }
        public async Task<T?> GetByIdAsync(TKey id) => await _dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) => await _dbSet.Where(predicate).ToListAsync();

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
              
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
             
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
           
        }

        //public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
