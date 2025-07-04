using System;
using System.Collections.Generic;

namespace SHARP.ViewModels.Form
{
	public class MdsFormDetailsModel : FormDetailsModel
	{
		public IReadOnlyCollection<MdsSectionModel> Sections { get; set;  }
        public IReadOnlyCollection<FormFieldModel> FormFields { get; set; }
    }
}

