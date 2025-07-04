using System;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Form
{
	public class MdsSectionDto
	{
        public int Id { get; set; }

        public string Name { get; set; }

        public IReadOnlyCollection<MdsGroupDto> Groups { get; set; }
    }
}

