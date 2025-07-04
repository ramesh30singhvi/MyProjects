using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        TEntity Add(TEntity entity);
        Task AddAsync(TEntity entity);

        void AddRange(IEnumerable<TEntity> entities);

        void Update(TEntity entity);
        void Update(TEntity entity, params string[] propertyNames);
        void UpdateRange(IEnumerable<TEntity> entities);

        void Remove<TKey>(TKey id);
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);

        int Count();
        int Count(Expression<Func<TEntity, bool>> predicate);
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);

        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
        TEntity GetSingleOrDefault(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> GetAsync<TKey>(TKey id);
        IQueryable<TEntity> GetAll();
        bool Any(Expression<Func<TEntity, bool>> predicate);

        DbSet<TEntity> GetDBSet();

        Task<IReadOnlyCollection<TEntity>> GetListAsync(
            Expression<Func<TEntity, bool>> predicate = null,
            Expression<Func<TEntity, object>> orderBySelector = null,
            bool ascending = true,
            bool asNoTracking = false,
            CancellationToken ct = default,
            Expression<Func<TEntity, object>> include = null);

        Task<TEntity> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, object>> include = null);

        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
    }
}
