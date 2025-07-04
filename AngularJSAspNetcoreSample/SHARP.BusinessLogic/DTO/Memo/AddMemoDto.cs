using System;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Memo
{
    public class AddMemoDto
    {
        public IReadOnlyCollection<int> OrganizationIds { get; set; }

        public string Text { get; set; }

        public DateTime? ValidityDate { get; set; }
    }
}
