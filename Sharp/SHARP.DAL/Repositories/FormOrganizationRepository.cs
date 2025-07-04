using Microsoft.EntityFrameworkCore;
using SHARP.Common.Enums;
using SHARP.Common.Extensions;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Extensions;
using SHARP.DAL.Models;
using SHARP.DAL.Models.QueryModels;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories
{
    public class FormOrganizationRepository : GenericRepository<FormOrganization>, IFormOrganizationRepository
    {
        public FormOrganizationRepository(SHARPContext context) : base(context)
        {
        }

        public async Task<FormOrganization[]> GetOrganizationFormsAsync(FormFilter filter, Expression<Func<FormOrganization, object>> orderBySelector)
        {
            var forms = GetFormOrganizationQuery(filter);

            return await forms
                .QueryOrderBy(orderBySelector, filter.SortOrder)
                .GetPagedAsync(filter.SkipCount, filter.TakeCount);
        }

        public async Task<IReadOnlyCollection<FilterOptionQueryModel>> GetDistinctColumnAsync(
            OrganizationFormFilterColumnSource<OrganizationFormFilterColumn> columnData,
            Expression<Func<FormOrganization, FilterOptionQueryModel>> columnSelector)
        {
            return await GetFormOrganizationQuery(columnData.FormFilter, columnData.Column)
                .Select(columnSelector)
                .Distinct()
                .ToArrayAsync();
        }

        private IQueryable<FormOrganization> GetFormOrganizationQuery(FormFilter filter, OrganizationFormFilterColumn? column = null)
        {
            var formOrganizations = GetFormOrganizationQuery()
                .Where(BuildFiltersCriteria(filter, column));

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                formOrganizations = formOrganizations.Where(formOrganization =>
                    formOrganization.Form.Name.Contains(filter.Search) ||
                    formOrganization.Form.AuditType.Name.Contains(filter.Search));
            }

            return formOrganizations;
        }

        private IQueryable<FormOrganization> GetFormOrganizationQuery()
        {
            return _context.FormOrganization
                .Include(formOrganization => formOrganization.Form)
                    .ThenInclude(form => form.AuditType)
                .Include(formOrganization => formOrganization.ScheduleSetting)
                .Where(formOrganization => formOrganization.Form.Versions.Any(formVersion => formVersion.Status != FormVersionStatus.Archived));
        }

        private Expression<Func<FormOrganization, bool>> BuildFiltersCriteria(FormFilter filter, OrganizationFormFilterColumn? column = null)
        {
            Expression<Func<FormOrganization, bool>> formExpr = PredicateBuilder.True<FormOrganization>();

            if (filter == null)
            {
                return formExpr;
            }

            var expression = PredicateBuilder
               .True<FormOrganization>()
               .AndIf(GetOrganizationExpression(filter), filter.OrganizationId.HasValue)
               .AndIf(GetNameExpression(filter), column != OrganizationFormFilterColumn.Name && filter.Name != null && filter.Name.Any())
               .AndIf(GetAuditTypeExpression(filter), column != OrganizationFormFilterColumn.AuditType && filter.AuditType != null && filter.AuditType.Any())
               .AndIf(GetSettingTypeExpression(filter), filter.SettingType.Any())
               .AndIf(GetScheduleTypeExpression(filter), filter.ScheduleSetting.Any())
               .AndIf(GetStateExpression(filter), filter.IsFormActive.Any());

            return expression;
        }

        private Expression<Func<FormOrganization, bool>> GetOrganizationExpression(FormFilter filter)
        {
            return i => i.OrganizationId == filter.OrganizationId;
        }

        private Expression<Func<FormOrganization, bool>> GetNameExpression(FormFilter filter)
        {
            return i => filter.Name.Select(option => option.Value).Contains(i.Form.Name);
        }

        private Expression<Func<FormOrganization, bool>> GetAuditTypeExpression(FormFilter filter)
        {
            return i => filter.AuditType.Select(option => option.Id).Contains(i.Form.AuditTypeId);
        }

        private Expression<Func<FormOrganization, bool>> GetSettingTypeExpression(FormFilter filter)
        {
            return i => filter.SettingType.Select(option => (FormSettingType)option.Id).Contains(i.SettingType);
        }

        private Expression<Func<FormOrganization, bool>> GetScheduleTypeExpression(FormFilter filter)
        {
            Expression<Func<FormOrganization, bool>> predicate = i => filter.ScheduleSetting.Select(option => (ScheduleType)option.Id).Contains(i.ScheduleSetting.ScheduleType);

            if (filter.ScheduleSetting.Select(option => (ScheduleType)option.Id).Contains(ScheduleType.None))
            {
                predicate = predicate.Or(i => i.ScheduleSetting == null);
            }

            return predicate;
        }

        private Expression<Func<FormOrganization, bool>> GetStateExpression(FormFilter filter)
        {
            return i => filter.IsFormActive.Select(option => option.Id == 1).Contains(i.Form.IsActive);
        }
    }
}