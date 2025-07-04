using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IBookmarkViewModel
    {
        Task<CPBookmarkListResponse> GetBookmarksAsync(int? id, int? status);
        Task<CPBookmarkListResponse> GetUserPrefBookmarkAsync(int userId, int businessId, int? status);
    }
}
