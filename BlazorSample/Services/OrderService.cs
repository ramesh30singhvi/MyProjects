using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class OrderService : IOrderService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        private static string _orderBaseUrl;
        public OrderService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
            _orderBaseUrl = _configuration["App:OrderApiUrl"];
        }
        public async Task<CreateOrderResponse> CreateOrderAsync(OrderRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<OrderRequestModel, CreateOrderResponse>(_orderBaseUrl + "Order/create-order", model);
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new CreateOrderResponse();
            }
        }

        public Task<GetOrderAddressByIDResponse> GetOrderAddressByIDAsync(int BusinessId, int orderId, int orderNumber)
        {
            throw new NotImplementedException();
        }

        public Task<GetOrderCustomerByIDResponse> GetOrderCustomerByIDAsync(int BusinessId, int orderId, int orderNumber)
        {
            throw new NotImplementedException();
        }

        public async Task<GetOrderDetailsResponse> GetOrderDetailsAsync(int BusinessId, int orderId, int orderNumber, string orderGUID)
        {
            try
            {
                return await _apiManager.GetAsync<GetOrderDetailsResponse>(string.Format("{0}Order/details/?BusinessId={1}&orderId={2}&orderNumber={3}&orderGUID={4}", _orderBaseUrl, BusinessId, orderId, orderNumber, orderGUID));
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new GetOrderDetailsResponse();
            }
        }

        public Task<GetOrderDiscountsByIDResponse> GetOrderDiscountsByIDAsync(int BusinessId, int orderId, int orderNumber)
        {
            throw new NotImplementedException();
        }

        public Task<GetOrderFeesByIDResponse> GetOrderFeesByIDAsync(int BusinessId, int orderId, int orderNumber)
        {
            throw new NotImplementedException();
        }

        public Task<GetOrderFulfillmentsByIDResponse> GetOrderFulfillmentsByIDAsync(int BusinessId, int orderId, int orderNumber)
        {
            throw new NotImplementedException();
        }

        public Task<GetOrderGiftCardsByIDResponse> GetOrderGiftCardsByIDAsync(int BusinessId, int orderId, int orderNumber)
        {
            throw new NotImplementedException();
        }

        public Task<GetOrderItemsByIDResponse> GetOrderItemsByIDAsync(int BusinessId, int orderId, int orderNumber)
        {
            throw new NotImplementedException();
        }

        public Task<GetOrderMetasByIDResponse> GetOrderMetasByIDAsync(int BusinessId, int orderId, int orderNumber)
        {
            throw new NotImplementedException();
        }

        public Task<GetOrderNotesByIDResponse> GetOrderNotesByIDAsync(int BusinessId, int orderNoteId, int orderId, int orderNumber)
        {
            throw new NotImplementedException();
        }

        public async Task<GetOrderSalesRepsByIDResponse> GetOrderSalesRepsByIDAsync(int BusinessId, int orderId, int orderNumber)
        {
            try
            {
                return await _apiManager.GetAsync<GetOrderSalesRepsByIDResponse>(string.Format("{0}Order/ordersalesrep?BusinessId={1}&orderId={2}&orderNumber={3}", _orderBaseUrl, BusinessId, orderId, orderNumber));
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new GetOrderSalesRepsByIDResponse();
            }
        }

        public Task<GetOrderTagsByIDResponse> GetOrderTagsByIDAsync(int BusinessId, int orderId, int orderNumber)
        {
            throw new NotImplementedException();
        }

        public Task<GetOrderTendersByIDResponse> GetOrderTendersByIDAsync(int BusinessId, int orderId, int orderNumber)
        {
            throw new NotImplementedException();
        }

        public Task<GetOrderTransactionsByIDResponse> GetOrderTransactionsByIDAsync(int BusinessId, int orderId, int orderNumber)
        {
            throw new NotImplementedException();
        }

        public async Task<GetSearchOrdersResponse> GetSearchOrdersAsync(int BusinessId, int OrderStatus, DateTime StartDate, DateTime EndDate, string searchText)
        {
            try
            {
                return await _apiManager.GetAsync<GetSearchOrdersResponse>(string.Format("{0}Order/list?BusinessId={1}&OrderStatus={2}&StartDate={3}&EndDate={4}&searchText={5}", _orderBaseUrl, BusinessId, OrderStatus, StartDate.ToString("MM-dd-yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture), EndDate.ToString("MM-dd-yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture), searchText));
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new GetSearchOrdersResponse();
            }
        }

        public async Task<CreateOrderResponse> UpdateOrderAsync(UpdateOrderRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<UpdateOrderRequestModel, CreateOrderResponse>(_orderBaseUrl + "Order/update-order", model);
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new CreateOrderResponse();
            }
        }

        public async Task<BaseResponse> UpdateOrderStatusAsync(UpdateOrderStatusRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<UpdateOrderStatusRequestModel, BaseResponse>(_orderBaseUrl + "Order/update-order-status", model);
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new BaseResponse();
            }
        }

        public async Task<AddUpdateOrderNoteResponse> AddUpdateOrderNoteAsync(AddUpdateOrderNoteRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<AddUpdateOrderNoteRequestModel, AddUpdateOrderNoteResponse>(_orderBaseUrl + "Order/add-update-note", model);
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new AddUpdateOrderNoteResponse();
            }
        }

        public async Task<AddUpdateOrderTagResponse> AddUpdateOrderTagAsync(AddUpdateOrderTagRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<AddUpdateOrderTagRequestModel, AddUpdateOrderTagResponse>(_orderBaseUrl + "Order/add-update-tag", model);
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new AddUpdateOrderTagResponse();
            }
        }

        public async Task<GetTagListResponse> GetTagList(int BusinessId)
        {
            try
            {
                return await _apiManager.GetAsync<GetTagListResponse>(_orderBaseUrl + "Order/tag-list?BusinessId=" + BusinessId);
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new GetTagListResponse();
            }
        }
        public async Task<BaseResponse> DeleteOrderSalesReps(DeleteOrderSalesRepsRequestModel model)
        {
            try
            {
                return await _apiManager.DeleteAsync<DeleteOrderSalesRepsRequestModel, BaseResponse>(_orderBaseUrl + "Order/delete-sales-reps", model);
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new BaseResponse();
            }
        }

        async Task<BaseResponse> IOrderService.DeleteOrderTag(DeleteOrderTagRequestModel model)
        {
            try
            {
                var response = await _apiManager.DeleteAsync<DeleteOrderTagRequestModel, BaseResponse>(_orderBaseUrl + "Order/delete-order-tags", model);
                return response;
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new BaseResponse();
            }
        }
        public async Task<BaseResponse> AddUpdateOrderSalesRepsAsync(AddUpdateOrderSalesRepsRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<AddUpdateOrderSalesRepsRequestModel, BaseResponse>(_orderBaseUrl + "Order/add-update-sales-reps", model);
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new BaseResponse();
            }
        }
        public async Task<EmailNotificationResponse> SendOrderEmailToBusQueue(EmailNotificationRequestModel requestModel)
        {
            try
            {
                return await _apiManager.PostAsync<EmailNotificationResponse>(_orderBaseUrl + "Order/send-order-email", requestModel);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new EmailNotificationResponse();
            }
        }

        public Task<BaseResponse> SendOrderReceiptAsync(SendOrderReceiptRequstModel requestModel)
        {
            throw new NotImplementedException();
        }

        public Task<int> AddOrderActivityAsync(AddOrderActivityRequestModel model)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UploadFileToCDN(string fileName, Stream file, string cdnContainerName)
        {
            throw new NotImplementedException();
        }

        public Task<UploadSignatureResponse> UploadSignatureFile(IFormFile file, int businessId)
        {
            throw new NotImplementedException();
        }

        public Task<DownloadSignatureResponse> DownloadSignatureFile(string fileName)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> DownloadFileFromCDN(string fileName, string cdnContainerName)
        {
            throw new NotImplementedException();
        }
        public async Task<PrintOrderReceiptResponse> PrintOrderReceiptPDF(PrintOrderReceiptPDFRequestModel requestModel)
        {
            try
            {
                return await _apiManager.PostAsync<PrintOrderReceiptResponse>(_orderBaseUrl + "Order/print-order-receipt-pdf", requestModel);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new PrintOrderReceiptResponse();
            }
        }
        public async Task<OrderMetafieldListResponse> AddUpdateOrderMetafieldListAsync(List<OrderMetafieldRequestModel> models)
        {
            try
            {
                return await _apiManager.PostAsync<OrderMetafieldListResponse>(_orderBaseUrl + "Order/add-update-order-metafields", models);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new OrderMetafieldListResponse();
            }
        }
        public async Task<CalculateSalesTaxResponse> CalculateTaxAsync(CalculateTaxRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<CalculateTaxRequestModel, CalculateSalesTaxResponse>(_orderBaseUrl + "Order/calculate-tax", model);
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new CalculateSalesTaxResponse();
            }
        }
    }
}
