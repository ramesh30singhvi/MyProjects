using System;
using System.Collections.Generic;

namespace SHARP.ViewModels.Memo
{
    public class MemoModel
    {
        public int Id { get; set; }

        public UserOptionModel User { get; set; }

        public IReadOnlyCollection<OptionModel> Organizations { get; set; }

        public string Text { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ValidityDate { get; set; }
    }
}
