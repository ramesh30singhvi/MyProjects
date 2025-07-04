using Microsoft.EntityFrameworkCore;
using SHARP.Common.Enums;
using SHARP.Common.Filtration;
using SHARP.DAL.Extensions;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories
{
    public class FormRepository : GenericRepository<Form>, IFormRepository
    {
        public FormRepository(SHARPContext context) : base(context)
        {
            
        }

        public async Task<IReadOnlyCollection<Form>> GetFormsByOrganizationAsync(int organizationId, string auditType, CancellationToken ct = default)
        {
            return await _entities
                .Include(form => form.AuditType)
                .Include(form => form.FormOrganizations)
                    .ThenInclude(formOrganization => formOrganization.Organization)
                .Where(form => form.FormOrganizations.Any(formOrganization => formOrganization.OrganizationId == organizationId))
                .Where(form => !string.IsNullOrEmpty(auditType) ? form.AuditType.Name == auditType : true)
                .Where(form => form.IsActive && form.Versions.Any(formVersion => formVersion.Status == FormVersionStatus.Published))
                .OrderBy(form => form.Name)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyCollection<Form>> GetFormOptionsAsync(FormOptionFilter filter)
        {
            var forms = _context.Form
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                forms = forms.Where(form =>
                    form.Name.Contains(filter.Search));
            }

            if (filter.OrganizationIds.Any())
            {
                forms = forms.Where(form => form.FormOrganizations.Any(formOrganization =>  filter.OrganizationIds.Contains(formOrganization.OrganizationId)));
            }

            return await forms
                .OrderBy(form => form.Name)
                .GetPagedAsync(filter.SkipCount, filter.TakeCount);
        }

        public async Task<List<Form>> GetFallFormsByOrganizationAsync(int organizationId)
        {
            var forms = _context.Form.AsQueryable();

            forms = forms.Where(form => form.OrganizationId == organizationId && form.Name.Contains("fall"));
            return await forms.ToListAsync();
        }

        public async Task<List<Form>> GetWoundFormsByOrganizationAsync(int organizationId)
        {
            var forms = _context.Form.AsQueryable();

            forms = forms.Where(form => form.OrganizationId == organizationId && form.Name.Contains("wound"));
            return await forms.ToListAsync();
        }
    }
}