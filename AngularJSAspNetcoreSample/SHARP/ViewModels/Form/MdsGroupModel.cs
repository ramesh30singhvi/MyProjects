using System;
using System.Collections.Generic;

namespace SHARP.ViewModels.Form
{
	public class MdsGroupModel
	{
        public string Id { get; set; }
        public string Name { get; set; }
        public int FormSectionId { get; set; }

        public IReadOnlyCollection<FormFieldModel> FormFields { get; set; }
    }
}

