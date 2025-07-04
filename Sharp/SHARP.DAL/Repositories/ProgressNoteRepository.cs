using Microsoft.EntityFrameworkCore;
using SHARP.Common.Constants;
using SHARP.Common.Extensions;
using SHARP.Common.Filtration;
using SHARP.DAL.Extensions;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories
{
    public class ProgressNoteRepository : GenericRepository<ProgressNote>, IProgressNoteRepository
    {
        public ProgressNoteRepository(SHARPContext context) : base(context)
        {
        }

        public async Task<int> GetCountKeywordsMatchingAsync(AuditProgressNoteFilter filter)
        {
            string[] keywords = filter.Keyword.Name.Split(CommonConstants.SLASH).Select(keyword => keyword.Trim()).ToArray();

            Expression<Func<ProgressNote, bool>> progressNoteExpression = GetProgressNoteKeywordExpression(filter);

            string[] notes = await _context.ProgressNote
                .Where(progressNoteExpression)
                .Select(note => note.ProgressNoteText)
                .ToArrayAsync();
                
            string aggregateNote = notes.Aggregate(new StringBuilder(), (current, next) => current.Append(" ").Append(next)).ToString();

            var regex = new Regex(@$"\b({keywords.Aggregate(new StringBuilder(), (current, next) => 
            !string.IsNullOrEmpty(current.ToString()) ? current.Append("|").Append(next) : current.Append("").Append(next))})\b", 
            RegexOptions.IgnoreCase);

            return regex.Matches(aggregateNote).Count;
        }

        public async Task<IReadOnlyCollection<ProgressNote>> GetProgresNotesAsync(AuditProgressNoteFilter filter)
        {
            Expression<Func<ProgressNote, bool>> progressNoteExpression = GetProgressNoteKeywordExpression(filter);

            return await _context.ProgressNote
                .Include(progressNote => progressNote.Patient)
                .Where(progressNoteExpression)
                .GetPagedAsync(filter.Skip, filter.Take);
        }

        private Expression<Func<ProgressNote, bool>> GetProgressNoteKeywordExpression(AuditProgressNoteFilter filter)
        {
            string[] keywords = filter.Keyword.Name?.Split(CommonConstants.SLASH).Select(keyword => keyword.Trim()).ToArray();

            DateTime dateFrom = filter.TimeZoneOffset.HasValue ? filter.DateFrom.AddHours(filter.TimeZoneOffset.Value * -1) : filter.DateFrom;
            DateTime dateTo = filter.TimeZoneOffset.HasValue ? filter.DateTo.Value.AddHours(filter.TimeZoneOffset.Value * -1) : filter.DateTo.Value;

            Expression <Func<ProgressNote, bool>> progressNoteExpression = progressNote =>
            progressNote.Patient.FacilityId == filter.FacilityId &&
            progressNote.EffectiveDate >= dateFrom && progressNote.EffectiveDate <= dateTo;

            Expression<Func<ProgressNote, bool>> expression = PredicateBuilder.False<ProgressNote>();
            if(keywords != null)
            {
                foreach (var keyword in keywords)
                {
                    expression = expression.Or(i => i.ProgressNoteText.Contains(keyword));
                }
            }

            return progressNoteExpression.And(expression);
        }
    }
}