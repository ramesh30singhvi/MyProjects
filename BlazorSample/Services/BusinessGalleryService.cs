using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class BusinessGalleryService : IBusinessGalleryService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        private static string _contentBaseUrl;

        public BusinessGalleryService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
            _contentBaseUrl = _configuration["App:ContentApiUrl"];
        }

        public async Task<List<BusinessGalleryViewModel>> GetBusinessGallery(int businessId, int businessProperty)
        {
            try
            {
                var response = await _apiManager.GetAsync<BusinessGalleryResponse>(string.Format("{0}BusinessGallery/{1}/{2}", _contentBaseUrl, businessId, businessProperty));
                return response.data;
            }
            catch(HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<BusinessGalleryViewModel>();
            }
        }

        public async Task<List<BusinessGalleryViewModel>> SaveGalleryImage(BusinessGalleryRequestViewModel businessGalleryRequest)
        {
            try
            {
                var response = await _apiManager.PostAsync<BusinessGalleryRequestViewModel, BusinessGalleryResponse>(string.Format("{0}BusinessGallery", _contentBaseUrl), businessGalleryRequest);
                return response.data;
            }
            catch(HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<BusinessGalleryViewModel>();
            }
        }

        public async Task<List<BusinessGalleryViewModel>> DeleteGalleryImage(int businessId, int imageId, int businessProperty)
        {
            try
            {
                var response = await _apiManager.DeleteAsync<BusinessGalleryResponse>(string.Format("{0}BusinessGallery/{1}/{2}/{3}", _contentBaseUrl,businessId,imageId, businessProperty));
                return response.data;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<BusinessGalleryViewModel>();
            }
        }
    }
}
