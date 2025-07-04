using SHARP.DAL.Models;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface ITableColumnRepository : IRepository<TableColumn>
    {
        Task<TableColumn> GetCriteriaQuestionAsync(int id);

        Task<TableColumn> GetTrackerQuestionAsync(int id);

        Task<TableColumn> GetKeywordAsync(int id);

        Task<TableColumn[]> GetFormKeywordsAsync(int formVersionId);

        Task<TableColumn[]> GetFormKeywordsAsyncForManagement(int formVersionId);

        Task<TableColumn[]> GetFormCriteriaQuestionsAsync(params int[] formVersionIds);

        Task<TableColumn[]> GetFormTrackerQuestionsAsync(params int[] formVersionIds);

        Task<TableColumn[]> GetFormCriteriaSimpleQuestionsAsync(params int[] formVersionIds);
    }
}
