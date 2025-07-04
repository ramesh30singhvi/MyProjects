using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class RSVPInviteRequest
    {
        public int member_id { get; set; } = 0;
        public int reservation_id { get; set; }
        public string invite_email { get; set; }
        public int guest_id { get; set; } = 0;
        public int user_id { get; set; } = 0;
        public Common.Common.InviteType invite_type { get; set; } = Common.Common.InviteType.Survey;
        public Common.Common.RSVPPostCaptureStatus invite_status { get; set; } = Common.Common.RSVPPostCaptureStatus.Available;
    }

    public class SurveyWaiverResponse : BaseResponse
    {
        public SurveyWaiverResponse()
        {
            data = new Model.SurveyWaiverStatus();
        }
        public Model.SurveyWaiverStatus data { get; set; }
    }
}
