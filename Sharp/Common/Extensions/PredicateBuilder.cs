using System;
using System.Linq.Expressions;

namespace SHARP.Common.Extensions
{
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            ParameterExpression p = expr1.Parameters[0];

            SubstExpressionVisitor visitor = new SubstExpressionVisitor();
            visitor.subst[expr2.Parameters[0]] = p;

            Expression body = Expression.OrElse(expr1.Body, visitor.Visit(expr2.Body));
            return Expression.Lambda<Func<T, bool>>(body, p);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
            return Expression.Lambda<Func<T, bool>>
                (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> AndIf<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2, bool condition)
        {
            if (condition)
            {
                return expr1.And(expr2);
            }

            return expr1;
        }

        public static Expression<Func<T, bool>> Inverse<T>(this Expression<Func<T, bool>> expr)
        {
            return Expression.Lambda<Func<T, bool>>(Expression.Not(expr.Body), expr.Parameters[0]);
        }
    }
}
