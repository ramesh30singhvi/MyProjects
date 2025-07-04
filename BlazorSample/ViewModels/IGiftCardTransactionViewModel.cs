using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IGiftCardTransactionViewModel
    {
        Task<GiftCardTransactionHistoryResponse> GetGiftCardTransactionHistory(int businessId, int orderId = 0, string giftCardGuid = "");
        Task<GiftCardDepletionResponse> GetGiftCardDepletionReport(int businessId, DateTime fromDate, DateTime toDate, string giftCardGuid = "");
    }
}
