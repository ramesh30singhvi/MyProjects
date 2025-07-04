using System;
using System.Linq.Expressions;

namespace SHARP.Common.Models
{
    public class OrderBy<TEntity>
    {
        public Expression<Func<TEntity, object>> Selector { get; set; }

        public bool ByDescending { get; set; } = false;
    }
}
