using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.DTO.Report
{
    public class SelectedDto
    {
        public IReadOnlyCollection<int> SelectedIds { get; set; }

        public IReadOnlyCollection<string> UserEmails { get; set; }
    }
}
