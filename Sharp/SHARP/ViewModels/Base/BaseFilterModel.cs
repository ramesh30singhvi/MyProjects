using System.Collections.Generic;

namespace SHARP.ViewModels.Base
{
    public class BaseFilterModel
    {
        public IReadOnlyCollection<string> Columns { get; set; } = new List<string>();

        public string OrderBy { get; set; }

        public string SortOrder { get; set; }

        public string Search { get; set; }

        public int TakeCount { get; set; } = 20;

        public int SkipCount { get; set; } = 0;
    }
}
