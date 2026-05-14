using global::SmartLogistics.Domain.Interfaces;
using global::SmartLogistics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartLogistics.Infrastructure.Repositories
{
    // كلاس الـ Unit of Work عشان نتحكم في كل المستودعات من مكان واحد
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>();
        private IDbContextTransaction? _transaction;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        // دالة بتجيب الـ Repository الخاص بكل Entity ولو مش موجود بتعمله Create
        public IRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);

            if (!_repositories.ContainsKey(type))
            {
                var repositoryInstance = new Repository<T>(_context);
                _repositories.Add(type, repositoryInstance);
            }

            return (IRepository<T>)_repositories[type];
        }

        

        // بدأ عملية Transaction (عشان لو حاجة فشلت نرجع في كلامنا)
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            // تمرير الـ token للـ context بيخلي العملية "بشرية" وذكية 
            // لو المستخدم كنسل الطلب، الداتا بيز هتوقف الحفظ فوراً
            return await _context.SaveChangesAsync(ct);
        }
    }
}