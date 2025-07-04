using System.Collections.Generic;
using System.Linq.Expressions;

namespace SHARP.Common.Extensions
{
    internal class SubstExpressionVisitor : ExpressionVisitor
    {
        public readonly Dictionary<Expression, Expression> subst = new Dictionary<Expression, Expression>();

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return subst.TryGetValue(node, out var newValue) ? newValue : node;
        }
    }
}
