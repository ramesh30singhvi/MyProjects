using System;

namespace SHARP.ViewModels.Portal
{
    public class PortalReportGridItemModel
    {

        public int Id { get; set; }


        public string OrganizationName { get; set; }

        public string FacilityName { get; set; }

        public int FacilityId { get; set; }

        public string ReportName { get; set; }


        public DateTime CreatedDate { get; set; }

        public string ReportCategoryName { get; set; }
        public string ReportRangeName { get; set; }

        public string ReportType { get; set; }
        public string UserName { get; set; }

        public int AuditId { get; set; }

        public int AuditType { get; set; }

        public int Compliance { get; set; }
    }
}
