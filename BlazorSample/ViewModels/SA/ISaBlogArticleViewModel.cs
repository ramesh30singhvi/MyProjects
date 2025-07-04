using CellarPassAppAdmin.Shared.Entities.v4;
using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels.SA
{
    public interface ISaBlogArticleViewModel
    {
        Task<CPBlogTagsListResponse> GetCPBlogTagsAsync(int? id, string name);
        Task<CPBlogTagsResponse> AddUpdateCPBlogTagsAsync(CPBlogTags model);
        Task<CPBlogCategoriesListResponse> GetCPBlogCategoriesAsync(int? id, string name);
        Task<CPBlogCategoriesResponse> AddUpdateCPBlogCategoriesAsync(CPBlogCategories model);
        Task<CPBlogArticlesResponse> AddUpdateCPBlogArticleAsync(BlogArticleRequestModel model);
        Task<CPBlogArticlesCategoryTreeResponse> GetCPBlogArticlesAsync();
        Task<CPBlogArticlesDetailResponse> GetCPBlogArticleDetailAsync(Guid idGuid);
        Task<CPBlogArticleBlockResponse> AddUpdateBlogArticleBlockAsync(CPBlogArticleBlockRequestModel model);
        Task<CPBlogArticleBlockResponse> GetCPBlogArticleBlockByIdAsync(int id);
        Task<CPBlogArticlesCategoryTreeResponse> DeleteCPBlogArticleAsync(Guid idGUID);
        Task<BlogArticleLeftRightBlockResponse> UpdateCPBlogArticleBlocksPositionAsync(List<BlogArticleBlocksPositionRequestModel> models);
        Task<BlogArticleLeftRightBlockResponse> GetCPBlogArticleBlocksAsync(int blogArticleId);
        Task<GenerateHTMLResponse> GenerateBlocksHTMLAsync(int blogArticleId);
    }
}
