using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SHARP.Common.Filtration
{
    public class ColumnQueryRule<T>
    {
        public Expression<Func<T, object>> SingleSelector { get; set; }

        public Expression<Func<T, IEnumerable<object>>> ManySelector { get; set; }
    }
}
