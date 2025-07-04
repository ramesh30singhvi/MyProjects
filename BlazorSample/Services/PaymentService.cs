using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        private static string _orderBaseUrl;

        public PaymentService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
            _orderBaseUrl = _configuration["App:OrderApiUrl"];
        }
        public async Task<TokenziedCardResponse> TokenzieCard(TokenizeCardRequestModel request)
        {
            try
            {
                return await _apiManager.PostAsync<TokenziedCardResponse>(_orderBaseUrl + "Payment/tokenzie-card", request);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new TokenziedCardResponse();
            }
        }
        public async Task<TokenziedCardListResponse> GetTokenizedCardsByCustomer(BusinessCustomerCardRequestModel request)
        {
            try
            {
                return await _apiManager.GetAsync<TokenziedCardListResponse>($"{_orderBaseUrl}Payment/tokenzie-cards-by-customer?businessCustomerId={request.BusinessCustomerId}&businessPaymentProfileId={request.BusinessPaymentProfileId}");
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new TokenziedCardListResponse();
            }
        }

        public async Task<ProcessCreditCardResponse> ProcessCreditCard(ProcessCreditCardRequestModel request)
        {
            throw new NotImplementedException();
        }
    }
}
