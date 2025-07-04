using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface ISalesOrderViewModel
    {
        Task<AddUpdateSalesOrderResponse> AddUpdateSalesOrder(OMSSettingsRequestModel model);
        Task<GetSalesOrderResponse> GetSalesOrderDetails(int businessPropertyId);
    }
}
