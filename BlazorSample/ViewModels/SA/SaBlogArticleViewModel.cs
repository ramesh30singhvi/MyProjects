using CellarPassAppAdmin.Shared.Entities.v4;
using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using CellarPassAppAdmin.Shared.Services.SA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels.SA
{
    public class SaBlogArticleViewModel : ISaBlogArticleViewModel
    {
        private readonly ISaBlogArticleService _saBlogArticleService;
        public SaBlogArticleViewModel(ISaBlogArticleService saBlogArticleService)
        {
            _saBlogArticleService = saBlogArticleService;
        }
        public async Task<CPBlogTagsListResponse> GetCPBlogTagsAsync(int? id, string name)
        {
            return await _saBlogArticleService.GetCPBlogTagsAsync(id, name);
        }
        public async Task<CPBlogTagsResponse> AddUpdateCPBlogTagsAsync(CPBlogTags model)
        {
            return await _saBlogArticleService.AddUpdateCPBlogTagsAsync(model);
        }
        public async Task<CPBlogCategoriesListResponse> GetCPBlogCategoriesAsync(int? id, string name)
        {
            return await _saBlogArticleService.GetCPBlogCategoriesAsync(id, name);
        }
        public async Task<CPBlogCategoriesResponse> AddUpdateCPBlogCategoriesAsync(CPBlogCategories model)
        {
            return await _saBlogArticleService.AddUpdateCPBlogCategoriesAsync(model);
        }
        public async Task<CPBlogArticlesResponse> AddUpdateCPBlogArticleAsync(BlogArticleRequestModel model)
        {
            return await _saBlogArticleService.AddUpdateCPBlogArticleAsync(model);
        }
        public async Task<CPBlogArticlesCategoryTreeResponse> GetCPBlogArticlesAsync()
        {
            return await _saBlogArticleService.GetCPBlogArticlesAsync();
        }
        public async Task<CPBlogArticlesDetailResponse> GetCPBlogArticleDetailAsync(Guid idGuid)
        {
            return await _saBlogArticleService.GetCPBlogArticleDetailAsync(idGuid);
        }
        public async Task<CPBlogArticleBlockResponse> AddUpdateBlogArticleBlockAsync(CPBlogArticleBlockRequestModel model)
        {
            return await _saBlogArticleService.AddUpdateBlogArticleBlockAsync(model);
        }
        public async Task<CPBlogArticleBlockResponse> GetCPBlogArticleBlockByIdAsync(int id)
        {
            return await _saBlogArticleService.GetCPBlogArticleBlockByIdAsync(id);
        }
        public async Task<CPBlogArticlesCategoryTreeResponse> DeleteCPBlogArticleAsync(Guid idGUID)
        {
            return await _saBlogArticleService.DeleteCPBlogArticleAsync(idGUID);
        }
        public async Task<BlogArticleLeftRightBlockResponse> UpdateCPBlogArticleBlocksPositionAsync(List<BlogArticleBlocksPositionRequestModel> models)
        {
            return await _saBlogArticleService.UpdateCPBlogArticleBlocksPositionAsync(models);
        }
        public async Task<BlogArticleLeftRightBlockResponse> GetCPBlogArticleBlocksAsync(int blogArticleId)
        {
            return await _saBlogArticleService.GetCPBlogArticleBlocksAsync(blogArticleId);
        }
        public async Task<GenerateHTMLResponse> GenerateBlocksHTMLAsync(int blogArticleId)
        {
            return await _saBlogArticleService.GenerateBlocksHTMLAsync(blogArticleId);
        }
    }
}
