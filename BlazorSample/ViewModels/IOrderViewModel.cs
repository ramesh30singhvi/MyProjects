using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IOrderViewModel
    {
        Task<GetSearchOrdersResponse> GetSearchOrdersAsync(int BusinessId, int OrderStatus, DateTime StartDate, DateTime EndDate, string searchText = "");
        Task<GetOrderDetailsResponse> GetOrderDetailsAsync(int BusinessId, int orderId, int orderNumber, string orderGUID);
        Task<GetOrderSalesRepsByIDResponse> GetOrderSalesRepsByIDAsync(int BusinessId, int orderId, int orderNumber);
        Task<CreateOrderResponse> CreateOrderAsync(OrderRequestModel model);
        Task<CreateOrderResponse> UpdateOrderAsync(UpdateOrderRequestModel model);
        Task<BaseResponse> UpdateOrderStatusAsync(UpdateOrderStatusRequestModel model);
        Task<AddUpdateOrderNoteResponse> AddUpdateOrderNoteAsync(AddUpdateOrderNoteRequestModel model);
        Task<AddUpdateOrderTagResponse> AddUpdateOrderTagAsync(AddUpdateOrderTagRequestModel model);
        Task<BaseResponse> AddUpdateOrderSalesReps(AddUpdateOrderSalesRepsRequestModel model);
        Task<BaseResponse> DeleteOrderTag(DeleteOrderTagRequestModel model);
        Task<BaseResponse> DeleteOrderSalesReps(DeleteOrderSalesRepsRequestModel model);
        Task<EmailNotificationResponse> SendOrderEmailToBusQueue(EmailNotificationRequestModel requestModel);
        Task<PrintOrderReceiptResponse> PrintOrderReceiptPDF(PrintOrderReceiptPDFRequestModel requestModel);
        Task<GetTagListResponse> GetTagList(int BusinessId);
        Task<OrderMetafieldListResponse> AddUpdateOrderMetafieldListAsync(List<OrderMetafieldRequestModel> models);
    }
}
