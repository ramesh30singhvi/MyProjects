using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class OrderSettingsViewModel : IOrderSettingsViewModel
    {
        private readonly IOrderSettingsService _orderSettingsService;
        public OrderSettingsViewModel(IOrderSettingsService orderSettingsService)
        {
            _orderSettingsService = orderSettingsService;
        }
        public async Task<CheckOrderStartingNumberResponse> CheckOrderCreatedByStartingNumber(int startingNumber)
        {
            return await _orderSettingsService.CheckOrderCreatedByStartingNumber(startingNumber);
        }
    }
}
