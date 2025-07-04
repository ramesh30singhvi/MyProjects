using CellarPassAppAdmin.Shared.Entities.v4;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IGuestTagViewModel
    {
        Task<CpTagsResponse> GetCpTagsAsync(int tagCategory);
        Task<CpTagTypesResponse> GetCpTagTypesAsync();
        Task<BusinessTagListResponse> GetBusinessTagAsync(int businessId);
        Task<BusinessTagResponse> GetBusinessTagByIdAsync(int id);
        Task<BusinessTagListResponse> AddUpdateBusinessTagAsync(BusinessTagRequestModel businessTag);
        Task<BusinessTagListResponse> DeleteBusinessTagByIdAsync(int id);
        Task<BaseResponse> SetBusinessTagPublicAsync(int id, bool isPublic);
    }
}
