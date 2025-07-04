using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class PaymentViewModel : IPaymentViewModel
    {
        private IPaymentService _paymentService;

        public PaymentViewModel(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public async Task<TokenziedCardResponse> TokenzieCard(TokenizeCardRequestModel request)
        {
            return await _paymentService.TokenzieCard(request);
        }
        public async Task<TokenziedCardListResponse> GetTokenizedCardsByCustomer(BusinessCustomerCardRequestModel request)
        {
            return await _paymentService.GetTokenizedCardsByCustomer(request);
        }
    }
}
