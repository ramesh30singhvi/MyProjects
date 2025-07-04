using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories
{
    internal class AuditAIPatientPdfNotesRepository : GenericRepository<AuditAIPatientPdfNotes>, IAuditAIPatientPdfNotesRepository
    {
        public AuditAIPatientPdfNotesRepository(SHARPContext context) : base(context)
        {
        }

        public async Task AddPatientValaues(List<AuditAIPatientPdfNotes> processedRedactedReports)
        {
            await _context.BulkInsertAsync<AuditAIPatientPdfNotes>(processedRedactedReports.ToList());
        }

        public async Task<AuditAIPatientPdfNotes> GetPatientNotes(int patientNotesId)
        {
            return await _context.AuditAIPatientPdfNotes
                .Include( audit => audit.Audit)
                    .ThenInclude( audit => audit.Facility)
                .Where( id => id.Id == patientNotesId ).FirstOrDefaultAsync();
        }
    }
}
