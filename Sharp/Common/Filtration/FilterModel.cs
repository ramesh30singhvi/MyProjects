using System.Collections.Generic;

namespace SHARP.Common.Filtration
{
    public abstract class FilterModel : SortModel
    {
        public IReadOnlyCollection<string> Columns { get; set; } = new List<string>();

        public string Search { get; set; }

        public int TakeCount { get; set; }

        public int SkipCount { get; set; }
    }
}
