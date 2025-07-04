using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class BookmarkViewModel : IBookmarkViewModel
    {
        private IBookmarkService _bookmarkService;

        public BookmarkViewModel(IBookmarkService bookmarkService)
        {
            _bookmarkService = bookmarkService;
        }
        public async Task<CPBookmarkListResponse> GetBookmarksAsync(int? id, int? status)
        {
            return await _bookmarkService.GetBookmarksAsync(id, status);
        }
        public async Task<CPBookmarkListResponse> GetUserPrefBookmarkAsync(int userId, int businessId, int? status)
        {
            return await _bookmarkService.GetUserPrefBookmarkAsync(userId, businessId, status);
        }
    }
}
