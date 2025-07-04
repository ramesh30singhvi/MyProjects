using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.ReportAI.Models
{
    public class PCCNotes
    {
        public string FacilityName { get; set; }
        public int FacilityId { get; set; }

        public string  PatientId { get; set; }
        public string PatientName { get; set; }

        public string DateTimeNotes { get; set; }
        public string Date { get; set; }

        public string Time { get; set; }

        public string PatientNotes { get; set; }

        public int ReportId { get; set; }
    }
}
