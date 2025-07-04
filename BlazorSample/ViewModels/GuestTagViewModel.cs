using CellarPassAppAdmin.Shared.Entities.v4;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class GuestTagViewModel : IGuestTagViewModel
    {
        private IGuestTagService _guestTagService;

        public GuestTagViewModel(IGuestTagService guestTagService)
        {
            _guestTagService = guestTagService;
        }
        /// <summary>
        /// Get All CP Tags
        /// </summary>
        /// <returns></returns>
        public async Task<CpTagsResponse> GetCpTagsAsync(int tagCategory)
        {
            return await _guestTagService.GetCpTagsAsync(tagCategory);
        }

        /// <summary>
        /// Get All CP Tag Types
        /// </summary>
        /// <returns></returns>
        public async Task<CpTagTypesResponse> GetCpTagTypesAsync()
        {
            return await _guestTagService.GetCpTagTypesAsync();
        }

        /// <summary>
        /// Get Business Tag by businessId
        /// </summary>
        /// <param name="businessId"></param>
        /// <returns>all buisness tags for businessId</returns>
        public async Task<BusinessTagListResponse> GetBusinessTagAsync(int businessId)
        {
            return await _guestTagService.GetBusinessTagAsync(businessId);
        }
        /// <summary>
        /// Get Business Tag By Id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>record for id</returns>
        public async Task<BusinessTagResponse> GetBusinessTagByIdAsync(int id)
        {
            return await _guestTagService.GetBusinessTagByIdAsync(id);
        }
        /// <summary>
        /// Add Update Business Tag
        /// </summary>
        /// <param name="businessTag">all request parameters</param>
        /// <returns>list of business tags</returns>
        public async Task<BusinessTagListResponse> AddUpdateBusinessTagAsync(BusinessTagRequestModel businessTag)
        {
            return await _guestTagService.AddUpdateBusinessTagAsync(businessTag);
        }

        /// <summary>
        /// Delete Business Tag By Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>list of business tags</returns>
        public async Task<BusinessTagListResponse> DeleteBusinessTagByIdAsync(int id)
        {
            return await _guestTagService.DeleteBusinessTagByIdAsync(id);
        }
        /// <summary>
        /// Set Business Tag Public or Private
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isPublic"></param>
        /// <returns></returns>
        public async Task<BaseResponse> SetBusinessTagPublicAsync(int id, bool isPublic)
        {
            return await _guestTagService.SetBusinessTagPublicAsync(id, isPublic);
        }
    }
}
