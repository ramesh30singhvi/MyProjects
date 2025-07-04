
using System;

namespace SHARP.ViewModels.Portal
{
    public class HighAlertGridItemModel
    {
        public int Id { get; set; }

        public string ReportName { get; set; }
        public string OrganizationName { get; set; }

        public string FacilityName { get; set; }

        public int FacilityId { get; set; }

        public string HighAlertDescription { get; set; }

        public string HighAlertCategoryPotentialAreas { get; set; }

        public string HighAlertNotes { get; set; }

        public string HighAlertStatus { get; set; }

        public int ReportTypeId { get; set; }

        public DateTime CreatedDate { get; set; }

        public string HighAlertCategoryName { get; set; }

        public string ChangedBy { get; set; }

        public string UserNotes { get; set; }

        public int Compliance { get; set; }
    }
}
