using SHARP.Common.Extensions;
using SHARP.DAL.Extensions;
using System;
using System.Linq.Expressions;

namespace SHARP.DAL.Helpers
{
    public static class SearchExpressionBuilder
    {
        public static Expression<Func<T, bool>> Build<T>(string searchQuery, params string[] properties)
        {
            Expression<Func<T, bool>> expression = PredicateBuilder.True<T>();

            string[] searches = searchQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var search in searches)
            {
                Expression<Func<T, bool>> subSearchResult = PredicateBuilder.False<T>();

                foreach (var propertyPath in properties)
                {
                    subSearchResult = subSearchResult.Or(ExpressionBuilder.BuildContainsExpression<T>(propertyPath, search));
                }

                expression = expression.And(subSearchResult);
            }

            return expression;
        }
    }
}
