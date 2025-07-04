using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IBusinessGalleryViewModel
    {
        Task<List<BusinessGalleryViewModel>> GetBusinessGallery(int businessId, int businessProperty);
        Task<List<BusinessGalleryViewModel>> SaveGalleryImage(BusinessGalleryRequestViewModel businessGalleryRequest);
        Task<List<BusinessGalleryViewModel>> DeleteGalleryImage(int businessId, int id, int businessProperty);
    }
}
