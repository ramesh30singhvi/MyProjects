using CPReservationApi.Model;
using System.Collections.Generic;


namespace CPReservationApi.WebApi.ViewModels
{
    public class UpdateReceiptSettingResponse : BaseResponse
    {
        public UpdateReceiptSettingResponse()
        {
            data = new WIneryReceiptResponseModel();
        }
        public WIneryReceiptResponseModel data { get; set; }
    }

    public class GetReceiptSettingResponse : BaseResponse
    {
        public GetReceiptSettingResponse()
        {
            data = new WineryReceiptSettingsWithLogo();
        }
        public WineryReceiptSettingsWithLogo data { get; set; }
    }

    public class UploadReceiptLogoResponse : BaseResponse
    {
        public UploadReceiptLogoResponse()
        {
            data = new UploadReceiptLogoResponseModel();
        }
        public UploadReceiptLogoResponseModel data { get; set; }
    }

    public class OrderSignatureResponse : BaseResponse
    {
        public OrderSignatureResponse()
        {
            data = new OrderSignatureResponseModel();
        }
        public OrderSignatureResponseModel data { get; set; }
    }
}
