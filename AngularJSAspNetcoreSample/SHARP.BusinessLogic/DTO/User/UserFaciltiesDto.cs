using System;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.User
{
	public class UserFaciltiesDto
	{
        public IReadOnlyCollection<OptionDto> Facilities { get; set; }

        public int? FilteredByUserId { get; set; }
    }
}

