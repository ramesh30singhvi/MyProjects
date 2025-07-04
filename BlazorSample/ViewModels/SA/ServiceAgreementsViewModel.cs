using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using CellarPassAppAdmin.Shared.Services.SA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels.SA
{
    public class ServiceAgreementsViewModel : IServiceAgreementsViewModel
    {
        private IServiceAgreementService _serviceAgreementService;
        public ServiceAgreementsViewModel(IServiceAgreementService serviceAgreementService)
        {
            _serviceAgreementService = serviceAgreementService;
        }

        public async Task<ServiceAgreementResponse> CreateServiceAgreement(ServiceAgreementModel model)
        {
            return await _serviceAgreementService.CreateServiceAgreement(model);
        }

        public async Task<ServiceAgreementsResponse> DeleteServiceAgreement(int Id)
        {
            return await _serviceAgreementService.DeleteServiceAgreement(Id);
        }

        public async Task<ServiceAgreementResponse> GetServiceAgreement(Guid IdGUID)
        {
            return await _serviceAgreementService.GetServiceAgreement(IdGUID);
        }

        public async Task<ServiceAgreementsResponse> GetServiceAgreements()
        {
            return await _serviceAgreementService.GetServiceAgreements();
        }
    }
}
