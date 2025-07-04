using System;
using System.Collections.Generic;

namespace SHARP.Common.Filtration
{
    public class MemoFilter
    {
        public IReadOnlyCollection<int> OrganizationIds { get; set; } = new List<int>();

        public DateTime CurrentDate { get; set; }
    }
}
