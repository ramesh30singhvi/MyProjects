using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class AlternateTemporaryLoginViewModel : IAlternateTemporaryLoginViewModel
    {
        private readonly IAlternateTemporaryLoginService _alternateTemporaryLoginService;

        public AlternateTemporaryLoginViewModel(IAlternateTemporaryLoginService alternateTemporaryLoginService)
        {
            _alternateTemporaryLoginService = alternateTemporaryLoginService;
        }
        public async Task<BaseResponse> SendAlternateTemporaryLoginLink(AlternateTemporaryLoginRequestModel model)
        {
            return await _alternateTemporaryLoginService.SendAlternateTemporaryLoginLink(model);
        }
    }
}
