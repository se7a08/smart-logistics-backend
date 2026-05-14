namespace SmartLogistics.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // الوصول لأي Repository
        IRepository<T> Repository<T>() where T : class;

        // حفظ التغييرات مع دعم إلغاء العملية (Async)
        Task<int> SaveChangesAsync(CancellationToken ct = default);

        // إدارة الـ Transactions
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}