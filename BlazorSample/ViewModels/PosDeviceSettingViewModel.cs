using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class PosDeviceSettingViewModel : IPOSDeviceSettingViewModel
    {
        private IPOSDeviceSettingService _pOSDeviceSettingService;
        public PosDeviceSettingViewModel(IPOSDeviceSettingService pOSDeviceSettingService)
        {
            _pOSDeviceSettingService = pOSDeviceSettingService;
        }
        public async Task<POSDeviceCodeResponse> GenerateDeviceCode(int codeLength)
        {
            return await _pOSDeviceSettingService.GenerateDeviceCode(codeLength);
        }

        public async Task<POSDeviceSettingViewModel> GetDeviceSettingDetails(int deviceId)
        {
            return await _pOSDeviceSettingService.GetDeviceSettingDetails(deviceId);
        }

        public async Task<POSDeviceDetailsResponse> SaveDeviceSettingDetails(POSDeviceSettingViewModel device)
        {
            return await _pOSDeviceSettingService.SaveDeviceSettingDetails(device);
        }
    }
}
