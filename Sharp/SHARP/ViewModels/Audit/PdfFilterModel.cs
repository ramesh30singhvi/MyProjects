namespace SHARP.ViewModels.Audit
{
    public class PdfFilterModel
    {
        public string AuditType { get; set; }

        public int? OrganizationId { get; set; }

        public int? FacilityId { get; set; }

        public int? FormId { get; set; }

        public string FromDate { get; set; }

        public string ToDate { get; set; }
    }
}
