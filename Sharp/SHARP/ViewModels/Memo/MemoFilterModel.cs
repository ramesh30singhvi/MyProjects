using SHARP.ViewModels.Base;
using System;
using System.Collections.Generic;

namespace SHARP.ViewModels.Memo
{
    public class MemoFilterModel : BaseFilterModel
    {
        public IReadOnlyCollection<int> OrganizationIds { get; set; } = new List<int>();
    }
}
