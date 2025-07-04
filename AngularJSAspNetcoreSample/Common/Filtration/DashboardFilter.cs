using SHARP.Common.Enums;
using System;
using System.Collections.Generic;

namespace SHARP.Common.Filtration
{
    public class DashboardFilter
    {
        public IReadOnlyCollection<int> Organizations { get; set; }

        public IReadOnlyCollection<int> Facilities { get; set; }

        public IReadOnlyCollection<int> Forms { get; set; }

        public DateFilterModel TimeFrame { get; set; }

        public DueDateType DueDateType { get; set; }

        public DateTime CurrentClientDate { get; set; }
    }
}
