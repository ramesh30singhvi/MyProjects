using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SHARP.DAL.Extensions
{
    public static class ExpressionBuilder
    {
        public static Expression<Func<T, bool>> BuildContainsExpression<T>(string propertyPath, string propertyValue)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T), "type");
            MemberExpression propertyExpression = GetMemberExpression(parameterExpression, propertyPath);
            MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            ConstantExpression value = Expression.Constant(propertyValue, typeof(string));
            MethodCallExpression containsMethodExp = Expression.Call(propertyExpression, method, value);

            return Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExpression);
        }

        public static Expression<Func<TEntity, bool>> BuildInRangeExpression<TEntity, TValue>(string propertyName, TValue fromValue, TValue toValue)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity), "type");
            MemberExpression propertyExpression = Expression.Property(parameterExpression, propertyName);

            BinaryExpression from = Expression.GreaterThanOrEqual(propertyExpression, Expression.Constant(fromValue, propertyExpression.Type));
            BinaryExpression to = Expression.LessThan(propertyExpression, Expression.Constant(toValue, propertyExpression.Type));
            var body = Expression.And(from, to);

            return Expression.Lambda<Func<TEntity, bool>>(body, parameterExpression);
        }

        public static Expression<Func<TEntity, bool>> BuildInRangeNumberExpression<TEntity, TValue>(string propertyName, TValue fromValue, TValue toValue)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity), "type");
            MemberExpression propertyExpression = Expression.Property(parameterExpression, propertyName);

            BinaryExpression from = Expression.GreaterThanOrEqual(propertyExpression, Expression.Constant(fromValue, propertyExpression.Type));
            BinaryExpression to = Expression.LessThanOrEqual(propertyExpression, Expression.Constant(toValue, propertyExpression.Type));
            var body = Expression.And(from, to);

            return Expression.Lambda<Func<TEntity, bool>>(body, parameterExpression);
        }

        public static Expression<Func<TEntity, bool>> BuildOutOfRangeExpression<TEntity, TValue>(string propertyName, TValue fromValue, TValue toValue)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity), "type");
            MemberExpression propertyExpression = Expression.Property(parameterExpression, propertyName);

            BinaryExpression from = Expression.LessThan(propertyExpression, Expression.Constant(fromValue, propertyExpression.Type));
            BinaryExpression to = Expression.GreaterThan(propertyExpression, Expression.Constant(toValue, propertyExpression.Type));
            var body = Expression.Or(from, to);

            return Expression.Lambda<Func<TEntity, bool>>(body, parameterExpression);
        }

        public static Expression<Func<TEntity, bool>> BuildLessThenExpression<TEntity, TValue>(string propertyName, TValue value)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity), "type");
            MemberExpression propertyExpression = Expression.Property(parameterExpression, propertyName);

            BinaryExpression body = Expression.LessThan(propertyExpression, Expression.Constant(value, propertyExpression.Type));

            return Expression.Lambda<Func<TEntity, bool>>(body, parameterExpression);
        }

        public static Expression<Func<TEntity, bool>> BuildGreaterThanExpression<TEntity, TValue>(string propertyName, TValue value)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity), "type");
            MemberExpression propertyExpression = Expression.Property(parameterExpression, propertyName);

            BinaryExpression body = Expression.GreaterThan(propertyExpression, Expression.Constant(value, propertyExpression.Type));

            return Expression.Lambda<Func<TEntity, bool>>(body, parameterExpression);
        }

        private static MemberExpression GetMemberExpression(ParameterExpression parameter, string propertyPath)
        {
            string[] propertyPathNodes = propertyPath.Split(".");
            MemberExpression result = Expression.Property(parameter, propertyPathNodes[0]);

            for (int i = 1; i < propertyPathNodes.Length; i++)
            {
                result = Expression.Property(result, propertyPathNodes[i]);
            }

            return result;
        }
    }
}
