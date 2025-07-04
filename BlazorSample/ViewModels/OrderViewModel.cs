using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class OrderViewModel : IOrderViewModel
    {
        private readonly IOrderService _orderService;
        public OrderViewModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<CreateOrderResponse> CreateOrderAsync(OrderRequestModel model)
        {
            return await _orderService.CreateOrderAsync(model);
        }

        public async Task<AddUpdateOrderNoteResponse> AddUpdateOrderNoteAsync(AddUpdateOrderNoteRequestModel model)
        {
            return await _orderService.AddUpdateOrderNoteAsync(model);
        }

        public async Task<BaseResponse> AddUpdateOrderSalesReps(AddUpdateOrderSalesRepsRequestModel model)
        {
            return await _orderService.AddUpdateOrderSalesRepsAsync(model);
        }


        public async Task<AddUpdateOrderTagResponse> AddUpdateOrderTagAsync(AddUpdateOrderTagRequestModel model)
        {
            return await _orderService.AddUpdateOrderTagAsync(model);
        }
        public async Task<GetTagListResponse> GetTagList(int BusinessId)
        {
            return await _orderService.GetTagList(BusinessId);
        }
        public async Task<BaseResponse> DeleteOrderSalesReps(DeleteOrderSalesRepsRequestModel model)
        {
            return await _orderService.DeleteOrderSalesReps(model);
        }

        public async Task<BaseResponse> DeleteOrderTag(DeleteOrderTagRequestModel model)
        {
            var response = await _orderService.DeleteOrderTag(model);
            return response;
        }


        public async Task<GetOrderDetailsResponse> GetOrderDetailsAsync(int BusinessId, int orderId, int orderNumber, string orderGUID)
        {
            return await _orderService.GetOrderDetailsAsync(BusinessId, orderId, orderNumber, orderGUID);
        }

        public async Task<GetOrderSalesRepsByIDResponse> GetOrderSalesRepsByIDAsync(int BusinessId, int orderId, int orderNumber)
        {
            return await _orderService.GetOrderSalesRepsByIDAsync(BusinessId, orderId, orderNumber);
        }


        public async Task<GetSearchOrdersResponse> GetSearchOrdersAsync(int BusinessId, int OrderStatus, DateTime StartDate, DateTime EndDate, string searchText)
        {
            return await _orderService.GetSearchOrdersAsync(BusinessId, OrderStatus, StartDate, EndDate, searchText);
        }

        public async Task<CreateOrderResponse> UpdateOrderAsync(UpdateOrderRequestModel model)
        {
            return await _orderService.UpdateOrderAsync(model);
        }

        public async Task<BaseResponse> UpdateOrderStatusAsync(UpdateOrderStatusRequestModel model)
        {
            return await _orderService.UpdateOrderStatusAsync(model);
        }
        public async Task<EmailNotificationResponse> SendOrderEmailToBusQueue(EmailNotificationRequestModel requestModel)
        {
            return await _orderService.SendOrderEmailToBusQueue(requestModel);
        }
        public async Task<PrintOrderReceiptResponse> PrintOrderReceiptPDF(PrintOrderReceiptPDFRequestModel requestModel)
        {
            return await _orderService.PrintOrderReceiptPDF(requestModel);
        }
        public async Task<OrderMetafieldListResponse> AddUpdateOrderMetafieldListAsync(List<OrderMetafieldRequestModel> models)
        {
            return await _orderService.AddUpdateOrderMetafieldListAsync(models);
        }
    }
}
