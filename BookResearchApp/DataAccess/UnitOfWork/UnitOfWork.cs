using BookResearchApp.Core.Entities;
using BookResearchApp.Core.Interfaces.Repositories;
using BookResearchApp.Data;
using BookResearchApp.DataAccess.Repository;

namespace BookResearchApp.DataAccess.UnitOfWork.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IBookRepository _books;
        private IAuthorRepository _authors;
        private IReviewRepository _reviews;
        private ICommentRepository _comments;
        private IUserRepository _users;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public void BeginTransaction()
        {
            _context.Database.BeginTransaction();
        }

        public void Commit()
        {
            _context.Database.CommitTransaction();
        }

        public void Rollback()
        {
            _context.Database.RollbackTransaction();
        }

        public IBookRepository Books => _books ??= new BookRepository(_context);
        public IAuthorRepository Authors => _authors ??= new AuthorRepository(_context);
        public IReviewRepository Reviews => _reviews ??= new ReviewRepository(_context);
        public ICommentRepository Comments => _comments ??= new CommentRepository(_context);
        public IUserRepository Users => _users ??= new UserRepository(_context);



        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }


        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
