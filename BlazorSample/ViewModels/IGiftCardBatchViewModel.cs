using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IGiftCardBatchViewModel
    {
        Task<AddBusinessGiftCardBatchResponse> AddBusinessGiftCardBatch(BusinessGiftCardBatchRequestModel model);
        Task<BusinessGiftCardBatchListResponse> GetBusinessGiftCardBatchList(int businessId);
        MemoryStream CreateBatchNumbersExcel(List<BusinessGiftCardBatchNumberModel> businessGiftCardBatchNumbers);
    }
}
