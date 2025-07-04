using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using uc = CPReservationApi.Common;

namespace CPReservationApi.WebApi.ViewModels
{
    public class BaseResponse
    {
        public BaseResponse()
        {
            error_info = new ErrorInfo();
        }
        public bool success { get; set; } = false;
        public ErrorInfo error_info { get; set; }
    }

    public class BaseResponse2
    {
        public BaseResponse2()
        {
            error_info = new ErrorInfo2();
        }
        public bool success { get; set; } = false;
        public ErrorInfo2 error_info { get; set; }
    }
}
