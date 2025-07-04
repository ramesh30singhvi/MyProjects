using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SHARP.Common.Filtration
{
    public class ColumnOptionQueryRule<T, TResult>
    {
        public Expression<Func<T, TResult>> SingleSelector { get; set; }

        public Expression<Func<T, IEnumerable<TResult>>> ManySelector { get; set; }
    }
}
