using CellarPassAppAdmin.Client.Exceptions;
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
    public class LegalArticleService : ILegalArticleService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        private static string _contentBaseUrl;
        public LegalArticleService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
            _contentBaseUrl = _configuration["App:ContentApiUrl"];
        }
        public async Task<LegalArticlesResponse> GetLegalArticles()
        {
            try
            {
                var response = await _apiManager.GetAsync<LegalArticlesResponse>(string.Format("{0}LegalArticles", _contentBaseUrl));
                return response;
            }catch(HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new LegalArticlesResponse();
            }
        }

        public async Task<LegalArticleResponse> CreateUpdateLegalArticle(LegalArticleModel model)
        {
            try
            {
                var response = await _apiManager.PostAsync<LegalArticleModel, LegalArticleResponse>(_contentBaseUrl + "LegalArticles/create-update-legal-article", model);
                return response;
            }catch(HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new LegalArticleResponse();
            }
        }

        public async Task<LegalArticleResponse> GetLegalArticleByGUID(Guid IdGUID)
        {
            try
            {
                var response = await _apiManager.GetAsync<LegalArticleResponse>(string.Format("{0}LegalArticles/get-legal-article/{1}", _contentBaseUrl, IdGUID));
                return response;
            }catch(HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new LegalArticleResponse();
            }
        }

        public async Task<LegalArticlesResponse> DeleteLegalArticle(int Id)
        {
            try
            {
                var response = await _apiManager.DeleteAsync<LegalArticlesResponse>(string.Format("{0}LegalArticles/{1}", _contentBaseUrl, Id));
                return response;
            }
            catch(HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new LegalArticlesResponse();
            }
        }
    }
}
