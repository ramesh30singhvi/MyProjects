using System;

namespace SHARP.Common.Filtration
{
    public class PdfFilter
    {
        public int? FacilityId { get; set; }

        public int FormId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public int? UserId { get; set; }
    }
}
