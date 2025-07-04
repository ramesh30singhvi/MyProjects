using SHARP.Common.Enums;
using System;
using System.Collections.Generic;

namespace SHARP.ViewModels.Dashboard
{
    public class DashboardFilterModel
    {
        public IReadOnlyCollection<int> Organizations { get; set; } = new List<int>();

        public IReadOnlyCollection<int> Facilities { get; set; } = new List<int>();

        public IReadOnlyCollection<int> Forms { get; set; } = new List<int>();

        public string TimeFrame { get; set; }

        public DueDateType DueDateType { get; set; }
    }
}
