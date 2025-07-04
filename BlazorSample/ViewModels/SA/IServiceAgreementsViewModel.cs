using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels.SA
{
    public interface IServiceAgreementsViewModel
    {
        Task<ServiceAgreementResponse> CreateServiceAgreement(ServiceAgreementModel model);
        Task<ServiceAgreementsResponse> GetServiceAgreements();
        Task<ServiceAgreementResponse> GetServiceAgreement(Guid IdGUID);
        Task<ServiceAgreementsResponse> DeleteServiceAgreement(int Id);
    }
}
