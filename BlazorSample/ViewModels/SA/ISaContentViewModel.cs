using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels.SA
{
    public interface ISaContentViewModel
    {
        Task<string> GetAdminLoginPageContentAsync();
        Task<CPContentResponse> GetCPContentByContentTypeAsync(int id, int contentType, string contentName);
        Task<CPContentResponse> AddUpdateCPContentAsync(CPContentRequestModel content);
        Task<CPContentNamesByContentTypeResponse> GetCPContentNamesByContentType(int contentType);
    }
}
