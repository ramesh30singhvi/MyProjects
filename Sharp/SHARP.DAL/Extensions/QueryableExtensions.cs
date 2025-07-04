using Microsoft.EntityFrameworkCore;
using SHARP.Common.Constants;
using SHARP.Common.Models;
using SHARP.DAL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SHARP.DAL.Extensions
{
    public static class QueryableExtensions
    {
        public static async Task<T[]> GetPagedAsync<T>(this IQueryable<T> query, int? skip, int? take)
        {
            if (!skip.HasValue || !take.HasValue || take == -1)
            {
                return await query.ToArrayAsync();
            }

            return await query
                .Skip(skip.Value)
                .Take(take.Value)
                .ToArrayAsync();
        }

        public static IQueryable<TEntity> QueryOrderBy<TEntity, TProperty>(
            this IQueryable<TEntity> query,
            Expression<Func<TEntity, TProperty>> selector,
            string sortOrder = CommonConstants.ASC_SORT_ORDER)
        {
            if (selector != null)
            {
                query = sortOrder == CommonConstants.DESC_SORT_ORDER ? query.OrderByDescending(selector) : query.OrderBy(selector);
            }

            return query;
        }

        public static IQueryable<TEntity> MultipleQueryOrderBy<TEntity>(this IQueryable<TEntity> query,
            ICollection<OrderBy<TEntity>> orderByModels)
        {
            if (orderByModels.Count > 0)
            {
                var orderedQuery = orderByModels.First().ByDescending
                    ? query.OrderByDescending(orderByModels.First().Selector)
                    : query.OrderBy(orderByModels.First().Selector);

                foreach (var orderByModel in orderByModels)
                {
                    if (orderByModel?.Selector != null && orderByModel.Selector != orderByModels.First().Selector)
                    {
                        orderedQuery = orderByModel.ByDescending ? orderedQuery.ThenByDescending(orderByModel.Selector) : orderedQuery.ThenBy(orderByModel.Selector);
                    }
                }

                query = orderedQuery;
            }

            return query;
        }

        public static IQueryable<TEntity> Search<TEntity>(this IQueryable<TEntity> query, string term, params string[] properties)
        {
            if (string.IsNullOrEmpty(term))
            {
                return query;
            }

            Expression<Func<TEntity, bool>> expression = SearchExpressionBuilder.Build<TEntity>(term, properties);
            return query.Where(expression);
        }

        public static async Task<TSource[]> GetPagedOptionsAsync<TSource>(this IQueryable<TSource> query, Expression<Func<TSource, object>> orderBySelector, int skip, int take)
        {
            query = query.OrderBy(orderBySelector);

            if (skip == 0 && take == 0)
            {
                return await query.ToArrayAsync();
            }

            return await query
                .Skip(skip)
                .Take(take)
                .ToArrayAsync();
        }
    }
}
