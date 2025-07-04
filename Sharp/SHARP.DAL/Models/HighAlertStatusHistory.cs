using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Models
{
    public  class HighAlertStatusHistory
    {
        public int Id { get; set; }

        public int HighAlertAuditValueId { get; set; }


        public int HighAlertStatusId { get; set; }

        public HighAlertStatus HighAlertStatus { get; set; }

        public string UserNotes { get; set; }

        public string ChangedBy { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
