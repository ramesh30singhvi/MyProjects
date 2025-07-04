using SHARP.Common.Filtration;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IProgressNoteRepository : IRepository<ProgressNote>
    {
        Task<IReadOnlyCollection<ProgressNote>> GetProgresNotesAsync(AuditProgressNoteFilter filter);

        Task<int> GetCountKeywordsMatchingAsync(AuditProgressNoteFilter filter);
    }
}
