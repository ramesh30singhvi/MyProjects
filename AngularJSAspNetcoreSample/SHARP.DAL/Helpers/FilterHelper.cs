using SHARP.Common.Enums;
using SHARP.Common.Extensions;
using SHARP.Common.Filtration;
using SHARP.DAL.Extensions;
using System;
using System.Linq.Expressions;

namespace SHARP.DAL.Helpers
{
    public static class FilterHelper
    {
        public static Expression<Func<TEntity, bool>> GetFilterExpression<TEntity, TValue>(string propertyName, Condition<TValue> firstCondition, Condition<TValue> secondCondition = null, Operator? filterOperator = null)
            where TValue : struct
        {
            Expression<Func<TEntity, bool>> firstConditionResult = GetConditionExpression<TEntity, TValue>(firstCondition, propertyName);

            if (filterOperator == null)
            {
                return firstConditionResult;
            }

            Expression<Func<TEntity, bool>> secondConditionResult = GetConditionExpression<TEntity, TValue>(secondCondition, propertyName);

            switch (filterOperator)
            {
                case Operator.And:
                    return firstConditionResult.And(secondConditionResult);
                case Operator.Or:
                    return firstConditionResult.Or(secondConditionResult);
                default:
                    throw new NotImplementedException();
            }
        }

        private static Expression<Func<TEntity, bool>> GetConditionExpression<TEntity, TValue>(Condition<TValue> condition, string propertyName)
             where TValue : struct
        {
            switch (condition.Type)
            {
                case CompareType.Equals:
                    return GetInRangeExpression<TEntity, TValue>(condition, propertyName);
                case CompareType.NotEqual:
                    return ExpressionBuilder.BuildOutOfRangeExpression<TEntity, TValue>(propertyName, condition.From, condition.To.Value);
                case CompareType.InRange:
                    return GetInRangeExpression<TEntity, TValue>(condition, propertyName);
                case CompareType.LessThan:
                    return ExpressionBuilder.BuildLessThenExpression<TEntity, TValue>(propertyName, condition.From);
                case CompareType.GreaterThan:
                    return ExpressionBuilder.BuildGreaterThanExpression<TEntity, TValue>(propertyName, condition.From);
                default:
                    throw new NotImplementedException();
            }
        }

        private static Expression<Func<TEntity, bool>> GetInRangeExpression<TEntity, TValue>(Condition<TValue> condition, string propertyName)
            where TValue : struct
        {
            if (typeof(TValue).Name is "DateTime")
            {
                return ExpressionBuilder.BuildInRangeExpression<TEntity, TValue>(propertyName, condition.From, condition.To.Value);
            }
            else
            {
                return ExpressionBuilder.BuildInRangeNumberExpression<TEntity, TValue>(propertyName, condition.From, condition.To.Value);
            }
        }
    }
}
