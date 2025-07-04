using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class DeltaDetailModel
    {
        public int id { get; set; }
    }
    public class DeltaResponse : BaseResponse
    {
        public DeltaResponse()
        {
            data = new DeltaDetailModel();
        }
        public DeltaDetailModel data { get; set; }
    }

    public class OpenDeviceSessionResponse : BaseResponse
    {
        public OpenDeviceSessionResponse()
        {
            data = new DeltaDetailModel();
        }
        public DeltaDetailModel data { get; set; }
    }

    public class CloseDeviceSessionResponse : BaseResponse
    {
        public CloseDeviceSessionResponse()
        {
            data = new DeltaDetailModel();
        }
        public DeltaDetailModel data { get; set; }
    }

    public class OpenDeviceResponse : BaseResponse
    {
        public OpenDeviceResponse()
        {
            data = new List<DeviceSessionModel>();
        }
        public List<DeviceSessionModel> data { get; set; }
    }

    public class OpenDeviceResponsev2 : BaseResponse
    {
        public OpenDeviceResponsev2()
        {
            data = new List<DeviceSessionModelV2>();
        }
        public List<DeviceSessionModelV2> data { get; set; }
    }

    public class CheckAndSendAppleNotificationResponse : BaseResponse
    {
        public CheckAndSendAppleNotificationResponse()
        {
            data = new List<DeltaModel>();
        }
        public List<DeltaModel> data { get; set; }
    }

    public class UpdateDeltaResponse : BaseResponse
    {
        public UpdateDeltaResponse()
        {
            data = new UpdateDelta();
        }
        public UpdateDelta data { get; set; }
    }

    public class UpdateDelta
    {
        public string device_id { get; set; }
    }

    public class AvailableNotificationsForDeviceResponse : BaseResponse
    {
        public AvailableNotificationsForDeviceResponse()
        {
            data = new List<AvailableNotificationsModel>();
        }
        public List<AvailableNotificationsModel> data { get; set; }
    }
}

