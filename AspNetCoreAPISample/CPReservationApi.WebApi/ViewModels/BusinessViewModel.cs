using CPReservationApi.Common;
using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class GetSubscriptionPlansResponse : BaseResponse
    {

        public GetSubscriptionPlansResponse()
        {
            data = new SubscriptionYearlyMonthlyPlans();
        }
        public SubscriptionYearlyMonthlyPlans data { get; set; }

    }

    public class GetDestinationLandingPageResponse : BaseResponse2
    {

        public GetDestinationLandingPageResponse()
        {
            data = new DestinationDetails();
        }
        public DestinationDetails data { get; set; }

    }

    public class ProfileDetailByMemberIdResponse : BaseResponse2
    {

        public ProfileDetailByMemberIdResponse()
        {
            data = new WidgetModel();
        }
        public WidgetModel data { get; set; }

    }

    public class WineryImagesResponse : BaseResponse2
    {

        public WineryImagesResponse()
        {
            data = new List<WineryImageModel>();
        }
        public List<WineryImageModel> data { get; set; }

    }

    public class GetSettingResponse : BaseResponse2
    {
        public GetSettingResponse()
        {
            data = new List<Settings.Setting>();
        }
        public List<Settings.Setting> data { get; set; }
    }

    public class WineryReviewResponse : BaseResponse2
    {
        public WineryReviewResponse()
        {
            data = new List<WineryReviewViewModel>();
        }
        public List<WineryReviewViewModel> data { get; set; }
    }
}
