using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using CellarPassAppAdmin.Shared.Services.SA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels.SA
{
    public interface ILegalArticleViewModel
    {
        Task<LegalArticlesResponse> GetLegalArticles();
        Task<LegalArticleResponse> CreateUpdateLegalArticle(LegalArticleModel model);
        Task<LegalArticleResponse> GetLegalArticleByGUID(Guid IdGUID);
        Task<LegalArticlesResponse> DeleteLegalArticle(int Id);
    }
}
