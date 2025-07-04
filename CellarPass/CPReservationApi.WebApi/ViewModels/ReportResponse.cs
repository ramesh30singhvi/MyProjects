using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class ShiftReportResponse : BaseResponse
    {

        public ShiftReportResponse()
        {
            data = new List<ShiftReport>();
        }
        public List<ShiftReport> data { get; set; }
    }

    public class CoversReportResponse : BaseResponse
    {

        public CoversReportResponse()
        {
            data = new List<CoversReportLocations>();
        }

        public List<CoversReportLocations> data { get; set; }
    }

    public class CoversReportResponseV2 : BaseResponse
    {

        public CoversReportResponseV2()
        {
            data = new List<CoversReportLocationsV2>();
        }

        public List<CoversReportLocationsV2> data { get; set; }
    }

    public class CoversReportResponseV3 : BaseResponse
    {

        public CoversReportResponseV3()
        {
            data = new List<CoversReportLocationsV3>();
        }

        public List<CoversReportLocationsV3> data { get; set; }
    }

    public class DailyReportResponse : BaseResponse
    {

        public DailyReportResponse()
        {
            data = new DailyReport();
        }

        public DailyReport data { get; set; }
    }

    public class ExportReservationDetailResponse : BaseResponse
    {
        public ExportReservationDetailResponse()
        {
            data = new List<ExportReservationDetail>();
        }

        public List<ExportReservationDetail> data { get; set; }
    }

    public class ExportReservationResponse : BaseResponse
    {
        public ExportReservationResponse()
        {
            data = new List<ExportReservation>();
        }

        public List<ExportReservation> data { get; set; }
    }
}
