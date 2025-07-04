using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Entities.v4;
using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using CellarPassAppAdmin.Shared.Services.SA;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services.SA
{
    public class SaBlogArticleService : ISaBlogArticleService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public SaBlogArticleService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<CPBlogTagsListResponse> GetCPBlogTagsAsync(int? id, string name)
        {
            try
            {
                return await _apiManager.GetAsync<CPBlogTagsListResponse>("SaBlogArticle/get-cp-blog-tags?id=" + id + "&name=" + name);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPBlogTagsListResponse();
            }
        }
        public async Task<CPBlogTagsResponse> AddUpdateCPBlogTagsAsync(CPBlogTags model)
        {
            try
            {
                return await _apiManager.PostAsync<CPBlogTagsResponse>("SaBlogArticle/add-update-cp-blog-tag", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPBlogTagsResponse();
            }
        }
        public async Task<CPBlogCategoriesListResponse> GetCPBlogCategoriesAsync(int? id, string name)
        {
            try
            {
                return await _apiManager.GetAsync<CPBlogCategoriesListResponse>("SaBlogArticle/get-cp-blog-categories?id=" + id + "&name=" + name);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPBlogCategoriesListResponse();
            }
        }
        public async Task<CPBlogCategoriesResponse> AddUpdateCPBlogCategoriesAsync(CPBlogCategories model)
        {
            try
            {
                return await _apiManager.PostAsync<CPBlogCategoriesResponse>("SaBlogArticle/add-update-cp-blog-category", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPBlogCategoriesResponse();
            }
        }
        public async Task<CPBlogArticlesResponse> AddUpdateCPBlogArticleAsync(BlogArticleRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<CPBlogArticlesResponse>("SaBlogArticle/add-update-cp-blog-article", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPBlogArticlesResponse();
            }
        }
        public async Task<CPBlogArticlesCategoryTreeResponse> GetCPBlogArticlesAsync()
        {
            try
            {
                return await _apiManager.GetAsync<CPBlogArticlesCategoryTreeResponse>("SaBlogArticle/get-cp-blog-articles");
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPBlogArticlesCategoryTreeResponse();
            }
        }
        public async Task<CPBlogArticlesDetailResponse> GetCPBlogArticleDetailAsync(Guid idGuid)
        {
            try
            {
                return await _apiManager.GetAsync<CPBlogArticlesDetailResponse>("SaBlogArticle/get-cp-blog-article-detail/" + idGuid);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPBlogArticlesDetailResponse();
            }
        }
        public async Task<CPBlogArticleBlockResponse> AddUpdateBlogArticleBlockAsync(CPBlogArticleBlockRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<CPBlogArticleBlockResponse>("SaBlogArticle/add-update-cp-blog-article-block", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPBlogArticleBlockResponse();
            }
        }

        public async Task<CPBlogArticleBlockResponse> GetCPBlogArticleBlockByIdAsync(int id)
        {
            try
            {
                return await _apiManager.GetAsync<CPBlogArticleBlockResponse>("SaBlogArticle/get-cp-blog-article-block/" + id);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPBlogArticleBlockResponse();
            }
        }
        public async Task<CPBlogArticlesCategoryTreeResponse> DeleteCPBlogArticleAsync(Guid idGUID)
        {
            try
            {
                return await _apiManager.DeleteAsync<CPBlogArticlesCategoryTreeResponse>("SaBlogArticle/delete-cp-blog-article/" + idGUID);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPBlogArticlesCategoryTreeResponse();
            }
        }
        public async Task<BlogArticleLeftRightBlockResponse> UpdateCPBlogArticleBlocksPositionAsync(List<BlogArticleBlocksPositionRequestModel> models)
        {
            try
            {
                return await _apiManager.PostAsync<BlogArticleLeftRightBlockResponse>("SaBlogArticle/update-cp-blog-article-blocks-position", models);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BlogArticleLeftRightBlockResponse();
            }
        }
        public async Task<BlogArticleLeftRightBlockResponse> GetCPBlogArticleBlocksAsync(int blogArticleId)
        {
            try
            {
                return await _apiManager.GetAsync<BlogArticleLeftRightBlockResponse>("SaBlogArticle/get-cp-blog-article-blocks/" + blogArticleId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BlogArticleLeftRightBlockResponse();
            }
        }
        public async Task<GenerateHTMLResponse> GenerateBlocksHTMLAsync(int blogArticleId)
        {
            try
            {
                return await _apiManager.GetAsync<GenerateHTMLResponse>("SaBlogArticle/generate-blocks-html/" + blogArticleId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GenerateHTMLResponse();
            }
        }
    }
}
