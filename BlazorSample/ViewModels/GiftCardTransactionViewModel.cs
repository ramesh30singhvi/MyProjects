using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class GiftCardTransactionViewModel : IGiftCardTransactionViewModel
    {
        private IProductService _productService;

        public GiftCardTransactionViewModel(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<GiftCardTransactionHistoryResponse> GetGiftCardTransactionHistory(int businessId, int orderId = 0, string giftCardGuid = "")
        {
            return await _productService.GetGiftCardTransactionHistory(businessId, orderId, giftCardGuid);
        }

        public async Task<GiftCardDepletionResponse> GetGiftCardDepletionReport(int businessId, DateTime fromDate, DateTime toDate, string giftCardGuid = "")
        {
            return await _productService.GetGiftCardDepletionReport(businessId, fromDate, toDate, giftCardGuid);
        }
    }
}
