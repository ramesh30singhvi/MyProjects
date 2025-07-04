using Microsoft.EntityFrameworkCore;
using SHARP.Common.Extensions;
using SHARP.Common.Filtration;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using SHARP.DAL.Extensions;
using SHARP.DAL.Helpers;
using SHARP.Common.Enums;
using SHARP.DAL.Models.QueryModels;
using SHARP.Common.Filtration.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Globalization;

namespace SHARP.DAL.Repositories
{
    public class FormVersionRepository : GenericRepository<FormVersion>, IFormVersionRepository
    {
        public FormVersionRepository(SHARPContext context) : base(context)
        {
        }

        public async Task<IReadOnlyCollection<FormVersion>> GetFormVersionsByOrganizationAsync(int organizationId, string auditType, CancellationToken ct = default)
        {
            return await _entities
                .Include(formVersion => formVersion.Form)
                    .ThenInclude(form => form.AuditType)
                .Include(formVersion => formVersion.Form)
                    .ThenInclude(form => form.FormOrganizations)
                    .ThenInclude(formOrganization => formOrganization.Organization)
                .Where(formVersion => formVersion.Form.FormOrganizations.Any(formOrganization=>formOrganization.OrganizationId == organizationId))
                .Where(formVersion => !string.IsNullOrEmpty(auditType) ? formVersion.Form.AuditType.Name == auditType : true)
                .Where(formVersion => formVersion.Status == FormVersionStatus.Published && formVersion.Form.IsActive)
                .OrderBy(formVersion => formVersion.Form.Name)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<FormVersion[]> GetAsync(FormVersionFilter filter, Expression<Func<FormVersion, object>> orderBySelector, List<int> maxFormVersionIds = null)
        {
            var formVersions = GetFormVersionQuery(filter);

            if (maxFormVersionIds != null && maxFormVersionIds.Any())
            {
                formVersions = formVersions
                .Where(formVersion => maxFormVersionIds.Contains(formVersion.Id));
            }

            formVersions = formVersions.Where(formVersion => formVersion.Form.IsActive);

            return orderBySelector != null ? await formVersions
                .QueryOrderBy(orderBySelector, filter.SortOrder)
                .GetPagedAsync(filter.SkipCount, filter.TakeCount) :  formVersions.ToArray();
        }

        public async Task<FormVersionQueryModel[]> GetFormVersionIdsAsync(FormVersionFilter filter)
        {
            var formVersions = GetFormVersionQuery(filter);

            return await formVersions
                .Select(formVersion => new FormVersionQueryModel() { Id = formVersion.Id, FormId = formVersion.FormId, Version = formVersion.Version })
                .ToArrayAsync();
        }

        public async Task<IReadOnlyCollection<FilterOptionQueryModel>> GetDistinctColumnAsync(FormManagementFilterColumnSource<FormVersionColumn> columnData, ColumnOptionQueryRule<FormVersion, FilterOptionQueryModel> columnQueryRule)
        {
            var formVersions = GetFormVersionQuery(columnData.FormManagementFilter, columnData.Column);

            IQueryable<FilterOptionQueryModel> columnValues;
            if (columnQueryRule.SingleSelector != null)
            {
                columnValues = formVersions.Select(columnQueryRule.SingleSelector);
            }
            else
            {
                columnValues = formVersions.SelectMany(columnQueryRule.ManySelector);
            }

            return await columnValues
                .Distinct()
                .ToArrayAsync();
        }

        public async Task<FormVersion> GetFormVersionKeywordsAsync(int id)
        {
            return await _context.FormVersion
                .Include(formVersion => formVersion.Form)
                    .ThenInclude(form => form.FormOrganizations)
                    .ThenInclude(formOrganization => formOrganization.Organization)
                .Include(formVersion => formVersion.Form)
                    .ThenInclude(form => form.AuditType)
                .Include(formVersion => formVersion.CreatedBy)

                .Include(formVersion => formVersion.Columns)
                   .ThenInclude(column => column.KeywordTrigger)
                   

                .FirstOrDefaultAsync(formVersion => formVersion.Id == id);
        }

        public async Task<FormVersion> GetFormVersionAsync(int id)
        {
            return await _context.FormVersion
                .Include(formVersion => formVersion.Form)
                    .ThenInclude(form => form.FormOrganizations)
                        .ThenInclude(formOrganization => formOrganization.Organization)
                .Include(formVersion => formVersion.Form)
                    .ThenInclude(form => form.AuditType)
                .Include(formVersion => formVersion.CreatedBy)

                .Include(formVersion => formVersion.Groups)

                .FirstOrDefaultAsync(form => form.Id == id);
        }

        public async Task<FormVersion> GetLastActiveFormVersionAsync(int formId)
        {
            return await _entities
                .Where(formVersion => formVersion.FormId == formId && formVersion.Status != FormVersionStatus.Archived)
                .OrderByDescending(formVersion => formVersion.Version)
                .FirstOrDefaultAsync();
        }

        private IQueryable<FormVersion> GetFormVersionQuery(FormVersionFilter filter, FormVersionColumn? column = null)
        {
            var formVersions = GetFormQuery();

            if (filter != null && !string.IsNullOrWhiteSpace(filter.Search) && formVersions.Any())
            {

                formVersions = formVersions.Where(formVersion =>
                        formVersion.Form.Name.Contains(filter.Search)
                         || formVersion.Form.FormOrganizations.Any(formOrganization => formOrganization.Organization.Name.Contains(filter.Search))
                         || formVersion.Form.AuditType.Name.Contains(filter.Search)); 
            }
            else
            {
               formVersions = formVersions.Where(BuildFiltersCriteria(filter, column));
            }

            return formVersions;
        }

        private Expression<Func<FormVersion, bool>> BuildFiltersCriteria(FormVersionFilter filter, FormVersionColumn? column = null)
        {
            Expression<Func<FormVersion, bool>> formVersionExpr = PredicateBuilder.True<FormVersion>();

            if (filter == null)
            {
                return formVersionExpr;
            }

            var expression = PredicateBuilder
            .True<FormVersion>()
               .AndIf(GetNameExpression(filter), column != FormVersionColumn.Name && filter.Name != null && filter.Name.Any())
               .AndIf(GetOrganizationsExpression(filter), column != FormVersionColumn.Organizations && filter.Organizations != null && filter.Organizations.Any())
               .AndIf(GetAuditTypeExpression(filter), column != FormVersionColumn.AuditType && filter.AuditType != null && filter.AuditType.Any());

            if (filter.CreatedDate != null)
            {
                expression = expression.And(GetCreatedDateExpression(filter));
            }


            return expression;
        }

        private Expression<Func<FormVersion, bool>> GetSearchWordInAuditTypeExpression(FormVersionFilter filter)
        {
            return i =>  i.Form.AuditType.Name.ToLower().Contains(filter.Search.ToLower());
        }

        private Expression<Func<FormVersion, bool>> GetSearchWordInNameExpression(FormVersionFilter filter)
        {
            return i =>  i.Form.Name.ToLower().Contains(filter.Search.ToLower());
        }
        private Expression<Func<FormVersion, bool>> GetSearchWordInOrganizationExpression(FormVersionFilter filter)
        {
            return i => i.Form.FormOrganizations
                         .Any(formOrganization => formOrganization.Organization.Name.ToLower().Contains(filter.Search.ToLower())); 
        }
        private Expression<Func<FormVersion, bool>> GetNameExpression(FormVersionFilter filter)
        {
            return i => filter.Name.Select(option => option.Id).Contains(i.Form.Id);
        }

        private Expression<Func<FormVersion, bool>> GetOrganizationsExpression(FormVersionFilter filter)
        {
            return i => i.Form.FormOrganizations
                        .Any(formOrganization => filter.Organizations.Select(option => option.Id).Contains(formOrganization.Organization.Id));
        }

        private Expression<Func<FormVersion, bool>> GetAuditTypeExpression(FormVersionFilter filter)
        {
            return i => filter.AuditType.Select(option => option.Id).Contains(i.Form.AuditType.Id);
        }

        private IQueryable<FormVersion> GetFormQuery()
        {
            return _context.FormVersion
                .Include(formVersion => formVersion.Form)
                    .ThenInclude(form => form.FormOrganizations)
                    .ThenInclude(formOrganization => formOrganization.Organization)
                .Include(formVersion => formVersion.Form)
                    .ThenInclude(form => form.AuditType);

        }

        private Expression<Func<FormVersion, bool>> GetCreatedDateExpression(FormVersionFilter filter)
        {
            return FilterHelper.GetFilterExpression<FormVersion, DateTime>(
                nameof(FormVersion.CreatedDate),
                filter.CreatedDate.FirstCondition,
                filter.CreatedDate.SecondCondition,
                filter.CreatedDate.Operator);
        }
    }
}