using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using CellarPassAppAdmin.Shared.Services.SA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels.SA
{
    public class SaContentViewModel : ISaContentViewModel
    {
        private ISaContentService _saContentService;

        public SaContentViewModel(ISaContentService saContentService)
        {
            _saContentService = saContentService;
        }
        public async Task<string> GetAdminLoginPageContentAsync()
        {
            return await _saContentService.GetAdminLoginPageContentAsync();
        }
        public async Task<CPContentResponse> GetCPContentByContentTypeAsync(int id, int contentType, string contentName)
        {
            return await _saContentService.GetCPContentByContentTypeAsync(id,contentType, contentName);
        }
        public async Task<CPContentResponse> AddUpdateCPContentAsync(CPContentRequestModel content)
        {
            return await _saContentService.AddUpdateCPContentAsync(content);
        }
        public async Task<CPContentNamesByContentTypeResponse> GetCPContentNamesByContentType(int contentType)
        {
            return await _saContentService.GetCPContentNamesByContentType(contentType);
        }
    }
}
