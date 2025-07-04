using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System.Threading;

namespace SHARP.DAL.Repositories
{
    public abstract class GenericRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly SHARPContext _context;
        protected readonly DbSet<TEntity> _entities;

        public GenericRepository(SHARPContext context)
        {
            _context = context;
            _entities = context.Set<TEntity>();
        }

        public virtual TEntity Add(TEntity entity)
        {
            var createdEntity = _entities.Add(entity);
            return createdEntity.Entity;
        }

        public async Task AddAsync(TEntity entity)
        {
            await _entities.AddAsync(entity);
        }

        public virtual void AddRange(IEnumerable<TEntity> entities)
        {
            _entities.AddRange(entities);
        }


        public virtual void Update(TEntity entity)
        {
            _entities.Update(entity);
        }

        public virtual void Update(TEntity entity, params string[] propertyNames)
        {
            _context.Entry(entity).State = EntityState.Unchanged;
            foreach (var property in propertyNames)
            {
                if (property.ToLower() == "id")
                    continue;
                _context.Entry(entity).Property(property).IsModified = true;
            }
        }

        public virtual void UpdateRange(IEnumerable<TEntity> entities)
        {
            _entities.UpdateRange(entities);
        }

        public void Remove<TKey>(TKey id)
        {
            var entity = _entities.Find(id);
            if (entity == null)
            {
                return;
            }
            _entities.Remove(entity);
        }

        public virtual void Remove(TEntity entity)
        {
            _entities.Remove(entity);
        }

        public virtual void RemoveRange(IEnumerable<TEntity> entities)
        {
            _entities.RemoveRange(entities);
        }


        public virtual int Count()
        {
            return _entities.Count();
        }

        public int Count(Expression<Func<TEntity, bool>> predicate)
        {
            return _entities.Count(predicate);
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _entities.CountAsync(predicate);
        }

        public virtual IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return _entities.Where(predicate);
        }

        public virtual TEntity GetSingleOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return _entities.SingleOrDefault(predicate);
        }

        public async virtual Task<TEntity> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, object>> include = null)
        {
            IQueryable<TEntity> query = _entities;

            if (include != null)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<TEntity> GetAsync<TKey>(TKey id) => await _entities.FindAsync(id).AsTask();

        public virtual IQueryable<TEntity> GetAll()
        {
            return _entities;
        }

        public virtual DbSet<TEntity> GetDBSet()
        {
            return _entities;
        }

        public bool Any(Expression<Func<TEntity, bool>> predicate)
        {
            return _entities.Any(predicate);
        }

        public virtual async Task<IReadOnlyCollection<TEntity>> GetListAsync(
            Expression<Func<TEntity, bool>> predicate = null,
            Expression<Func<TEntity, object>> orderBySelector = null,
            bool ascending = true,
            bool asNoTracking = false,
            CancellationToken ct = default,
            Expression<Func<TEntity, object>> include = null)
        {
            IQueryable<TEntity> query = _entities;

            if (include != null)
            {
                query = query.Include(include);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (orderBySelector != null && ascending)
            {
                query = query.OrderBy(orderBySelector);
            }
            else if (orderBySelector != null && !ascending)
            {
                query = query.OrderByDescending(orderBySelector);
            }

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query
                .ToListAsync(ct);
        }

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _entities.AnyAsync(predicate);
        }
    }
}
