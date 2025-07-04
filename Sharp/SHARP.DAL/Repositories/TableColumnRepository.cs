using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories
{
    public class TableColumnRepository : GenericRepository<TableColumn>, ITableColumnRepository
    {
        public TableColumnRepository(SHARPContext context) : base(context)
        {
        }

        public async Task<TableColumn> GetCriteriaQuestionAsync(int id)
        {
            return await _context.TableColumn
                .Include(column => column.Group)
                .Include(column => column.CriteriaOption)
                .FirstOrDefaultAsync(column => column.Id == id);
        }

        public async Task<TableColumn> GetTrackerQuestionAsync(int id)
        {
            return await _context.TableColumn
                .Include(column => column.TrackerOption)
                    .ThenInclude(trackerOption => trackerOption.Items)
                .Include(column => column.TrackerOption)
                    .ThenInclude(trackerOption => trackerOption.FieldType)
                .FirstOrDefaultAsync(column => column.Id == id);
        }

        public async Task<TableColumn[]> GetFormKeywordsAsync(int formVersionId)
        {
            return await _context.TableColumn
                .Where(column => column.FormVersionId == formVersionId)
                .Include(column => column.KeywordTrigger)
                    .ThenInclude(key => key.FormTriggeredByKeyword)
                .ToArrayAsync();

        }

        public async Task<TableColumn[]> GetFormKeywordsAsyncForManagement(int formVersionId)
        {
            var columns =  await _context.TableColumn
                .Where(column => column.FormVersionId == formVersionId && column.Hidden == null)
                .Include(column => column.KeywordTrigger)
                    .ThenInclude( key => key.FormTriggeredByKeyword)
                .ToArrayAsync();

            return columns;

        }

        public async Task<TableColumn[]> GetFormCriteriaQuestionsAsync(params int[] formVersionIds)
        {
            return await _context.TableColumn
                .Include(column => column.Group)
                .Include(column => column.CriteriaOption)
                .Where(column => formVersionIds.Contains(column.FormVersionId))
                .ToArrayAsync();
        }

        public async Task<TableColumn[]> GetFormCriteriaSimpleQuestionsAsync(params int[] formVersionIds)
        {
            return await _context.TableColumn
                .Where(column => formVersionIds.Contains(column.FormVersionId))
                .ToArrayAsync();
        }

        public async Task<TableColumn[]> GetFormTrackerQuestionsAsync(params int[] formVersionIds)
        {
            return await _context.TableColumn
                .Include(column => column.TrackerOption)
                    .ThenInclude(trackerOption => trackerOption.Items)
                .Include(column => column.TrackerOption)
                    .ThenInclude(trackerOption => trackerOption.FieldType)
                .Where(column => formVersionIds.Contains(column.FormVersionId))
                .ToArrayAsync();
        }

        public async Task<TableColumn> GetKeywordAsync(int id)
        {
            return await _context.TableColumn
                .Include(column => column.KeywordTrigger)
                    .ThenInclude(key => key.FormTriggeredByKeyword)
                .FirstOrDefaultAsync(column => column.Id == id); 
        }
    }
}
