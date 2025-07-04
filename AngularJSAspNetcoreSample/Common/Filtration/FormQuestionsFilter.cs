using System;
using System.Collections.Generic;

namespace SHARP.Common.Filtration
{
	public class FormQuestionsFilter : FilterModel
	{
        public IReadOnlyCollection<int> FormVersionIds { get; set; }
    }
}

