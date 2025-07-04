using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using CellarPassAppAdmin.Shared.Services.SA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels.SA
{
    public class LegalArticleViewModel : ILegalArticleViewModel
    {
        private ILegalArticleService _legalArticleService;
        public LegalArticleViewModel(ILegalArticleService legalArticleService)
        {
            _legalArticleService = legalArticleService;
        }

        public async Task<LegalArticleResponse> CreateUpdateLegalArticle(LegalArticleModel model)
        {
            return await _legalArticleService.CreateUpdateLegalArticle(model);
        }

        public async Task<LegalArticlesResponse> DeleteLegalArticle(int Id)
        {
            return await _legalArticleService.DeleteLegalArticle(Id);
        }

        public async Task<LegalArticleResponse> GetLegalArticleByGUID(Guid IdGUID)
        {
            return await _legalArticleService.GetLegalArticleByGUID(IdGUID);
        }

        public async Task<LegalArticlesResponse> GetLegalArticles()
        {
            return await _legalArticleService.GetLegalArticles();
        }
    }
}
