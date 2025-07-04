using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class BusinessgalleryViewModel : IBusinessGalleryViewModel
    {
        private IBusinessGalleryService _businessGalleryService;
        public BusinessgalleryViewModel(IBusinessGalleryService businessGalleryService)
        {
            _businessGalleryService = businessGalleryService;
        }
        public async Task<List<BusinessGalleryViewModel>> DeleteGalleryImage(int businessId, int id, int businessProperty)
        {
            return await _businessGalleryService.DeleteGalleryImage(businessId, id, businessProperty);
        }

        public async Task<List<BusinessGalleryViewModel>> GetBusinessGallery(int businessId, int businessProperty)
        {
            return await _businessGalleryService.GetBusinessGallery(businessId, businessProperty);
        }

        public async Task<List<BusinessGalleryViewModel>> SaveGalleryImage(BusinessGalleryRequestViewModel businessGalleryRequest)
        {
            return await _businessGalleryService.SaveGalleryImage(businessGalleryRequest);
        }
    }
}
