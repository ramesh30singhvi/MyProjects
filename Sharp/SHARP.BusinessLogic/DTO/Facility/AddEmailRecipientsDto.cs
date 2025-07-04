using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.DTO.Facility
{
    public class AddEmailRecipientsDto
    {
        public int? Id { get; set; }

        public IReadOnlyCollection<string> Emails { get; set; }
    }
}
