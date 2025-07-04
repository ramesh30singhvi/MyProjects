using System;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Form
{
	public class MdsFormDetailsDto : FormVersionDto
	{
		public IReadOnlyCollection<MdsSectionDto> Sections { get; set; }
        public IReadOnlyCollection<FormFieldDto> FormFields { get; set; }

    }
}

