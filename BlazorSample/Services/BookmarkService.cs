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
    public class BookmarkService : IBookmarkService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public BookmarkService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<CPBookmarkListResponse> GetBookmarksAsync(int? id, int? status)
        {
            try
            {
                return await _apiManager.GetAsync<CPBookmarkListResponse>(_configuration["App:PeopleApiUrl"] + "Bookmark/get-bookmarks?id=" + id + "&status=" + status);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPBookmarkListResponse();
            }
        }
        public async Task<CPBookmarkListResponse> GetUserPrefBookmarkAsync(int userId, int businessId, int? status)
        {
            try
            {
                return await _apiManager.GetAsync<CPBookmarkListResponse>(_configuration["App:PeopleApiUrl"] + "Bookmark/get-user-prefered-bookmarks/" + userId + "/" + businessId + "?status=" + status);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPBookmarkListResponse();
            }
        }
    }
}
