using System;
using SHARP.BusinessLogic.DTO.Audit;

namespace SHARP.BusinessLogic.DTO.Form
{
	public class AddMdsGroupDto : MdsGroupDto
    {
        public int FormVersionId { get; set; }
    }
}

