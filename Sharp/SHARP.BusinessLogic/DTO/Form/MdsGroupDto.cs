using System;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Form
{
	public class MdsGroupDto
	{
        public int Id { get; set; }

        public string Name { get; set; }

        public int FormSectionId { get; set; }

        public IReadOnlyCollection<FormFieldDto> FormFields { get; set; }
    }
}

