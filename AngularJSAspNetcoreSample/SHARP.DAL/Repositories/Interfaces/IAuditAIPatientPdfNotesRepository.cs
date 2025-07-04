using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IAuditAIPatientPdfNotesRepository : IRepository<AuditAIPatientPdfNotes>
    {
        Task AddPatientValaues(List<AuditAIPatientPdfNotes> processedRedactedReports);
        Task<AuditAIPatientPdfNotes> GetPatientNotes(int patientNotesId);
    }
}
