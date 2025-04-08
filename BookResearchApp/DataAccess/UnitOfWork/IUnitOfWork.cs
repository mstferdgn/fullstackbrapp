using BookResearchApp.Core.Interfaces.Repositories;

namespace BookResearchApp.DataAccess.UnitOfWork.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        void BeginTransaction();
        void Commit();
        void Rollback();

        IBookRepository Books { get; }
        IAuthorRepository Authors { get; }
        ICommentRepository Comments { get; }
        IReviewRepository Reviews { get; }
        IUserRepository Users { get; }

        Task<int> SaveChangesAsync();
    }
}
