using System;
using System.Collections.Generic;

namespace SHARP.ViewModels.Memo
{
    public class AddMemoModel
    {
        public IReadOnlyCollection<int> OrganizationIds { get; set; }

        public string Text { get; set; }

        public DateTime? ValidityDate { get; set; }
    }
}
