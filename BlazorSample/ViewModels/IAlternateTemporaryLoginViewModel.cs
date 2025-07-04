using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IAlternateTemporaryLoginViewModel
    {
        Task<BaseResponse> SendAlternateTemporaryLoginLink(AlternateTemporaryLoginRequestModel model);
    }
}
