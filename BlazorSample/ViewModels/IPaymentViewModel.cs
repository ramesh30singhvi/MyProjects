using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IPaymentViewModel
    {
        Task<TokenziedCardResponse> TokenzieCard(TokenizeCardRequestModel request);
        Task<TokenziedCardListResponse> GetTokenizedCardsByCustomer(BusinessCustomerCardRequestModel request);
    }
}
