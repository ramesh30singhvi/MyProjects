using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic;
using SHARP.Common.Enums;
using SHARP.Common.Extensions;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Extensions;
using SHARP.DAL.Helpers;
using SHARP.DAL.Models;
using SHARP.DAL.Models.QueryModels;
using SHARP.DAL.Repositories.Interfaces;

namespace SHARP.DAL.Repositories
{
    public class AuditRepository : GenericRepository<Audit>, IAuditRepository
    {
        public AuditRepository(SHARPContext context) : base(context)
        {
        }

        public async Task<Tuple<Audit[], int>> GetListAsync(AuditFilter filter, Expression<Func<Audit, object>> orderBySelector)
        {
            var query = GetAuditQuery()
                .Where(a=>a.State == filter.State)
                .Where(BuildOrganizationCriteria(filter.UserOrganizationIds))
                .Where(BuildFiltersCriteria(filter))
                .QueryOrderBy(orderBySelector, filter.SortOrder);

            int totalCount = query.Count();
            var audits = await query.GetPagedAsync(filter.SkipCount, filter.TakeCount);
            return new Tuple<Audit[], int>(audits, totalCount);
        }

        private IQueryable<Audit> GetAuditForDownloadQuery()
        {
            return _context.Audit.Include(audit => audit.SubmittedByUser)
                .Include(audit => audit.Facility)
                        .ThenInclude(facility => facility.Organization)

                .Include(audit => audit.FormVersion)
                    .ThenInclude(formVersion => formVersion.Form)
                .Include(audit => audit.Values)

                .Include(audit => audit.AuditFieldValues)

                .Include(audit => audit.Settings);
        }
        public async Task<Audit> GetAuditForDownloadAsync(int id)
        {
            var query = GetAuditForDownloadQuery();

            return await query.Where(audit => audit.Id == id).FirstOrDefaultAsync();
        }
        public async Task<Audit> GetAuditAsync(int id)
        {
            return await _context.Audit
                .Include(audit => audit.SubmittedByUser)
                .Include(audit => audit.Facility)
                        .ThenInclude(facility => facility.Organization)
                .Include(audit => audit.Facility)
                        .ThenInclude(facility => facility.TimeZone)
                .Include(audit => audit.FormVersion)
                    .ThenInclude(formVersion => formVersion.Form)

                .Include(audit => audit.HighAlertAuditValues)
                    .ThenInclude(highAlert => highAlert.HighAlertCategory)
                .Include(audit => audit.HighAlertAuditValues)
                    .ThenInclude(highAlert => highAlert.HighAlertStatusHistory)
                        .ThenInclude(highAlert => highAlert.HighAlertStatus)

                .Include(audit => audit.Values)
                    .ThenInclude(auditvalue => auditvalue.HighAlertAuditValue)
                        .ThenInclude(highAlert => highAlert.HighAlertCategory)
                .Include(audit => audit.Values)
                    .ThenInclude(auditvalue => auditvalue.HighAlertAuditValue)
                     .ThenInclude(highAlert => highAlert.HighAlertStatusHistory)
                        .ThenInclude(highAlert => highAlert.HighAlertStatus)

                .Include(audit => audit.AuditFieldValues)

                .Include(audit => audit.Settings)
                .Include(audit => audit.PortalReport)

                .FirstOrDefaultAsync(audit => audit.Id == id);
        }

        public async Task<Audit> GetAuditWithTypeAsync(int id)
        {
            return await _context.Audit
                .Include(audit => audit.FormVersion)
                    .ThenInclude(formVersion => formVersion.Form)
                        .ThenInclude(form => form.AuditType)
                .FirstOrDefaultAsync(audit => audit.Id == id);
        }

        public async Task<bool> ExistsSubmittedAuditsAsync(PdfFilter filter)
        {
            IQueryable<Audit> query = GetSubmittedAuditsQuery(filter);

            return await query.AnyAsync();
        }

        public async Task<IReadOnlyCollection<Audit>> GetSubmittedAuditsAsync(PdfFilter filter)
        {
            IQueryable<Audit> query = GetSubmittedAuditsQuery(filter);

            return await query
                .AsNoTracking()
                .ToArrayAsync();
        }

        private IQueryable<Audit> GetSubmittedAuditsQuery(PdfFilter filter)
        {
            var dateTo = filter.ToDate.HasValue ? filter.ToDate.Value : filter.FromDate.Value;

            IQueryable<Audit> query = GetAuditDetailsQuery()
                .Where(audit =>
                    audit.Status == AuditStatus.Submitted &&
                    audit.State == AuditState.Active &&
                    audit.FormVersion.FormId == filter.FormId &&
                    audit.IncidentDateFrom >= filter.FromDate &&
                    audit.IncidentDateFrom <= dateTo);

            if (filter.FacilityId.HasValue)
            {
                query = query.Where(audit => audit.FacilityId == filter.FacilityId);
            }

            if (filter.UserId.HasValue)
            {
                query = query.Where(audit => audit.SubmittedByUser.Id == filter.UserId);
            }

            return query;
        }

        public async Task<IReadOnlyCollection<FilterOptionQueryModel>> GetDistinctColumnAsync(AuditFilterColumnSource<AuditFilterColumn> columnData, Expression<Func<Audit, FilterOptionQueryModel>> columnSelector)
        {
            var query = GetAuditQuery()
                .Where(audit=>audit.State == columnData.AuditFilter.State)
                .Where(BuildOrganizationCriteria(columnData.UserOrganizationIds))
                .Where(BuildFiltersCriteria(columnData.AuditFilter, columnData.Column))
                .Select(columnSelector)
                .Distinct();
            /*.OrderBy(i => i.Value);

            if (columnSelector.Body.Type == typeof(FilterOptionQueryModel))
            {
                query = query.ThenBy(i => i.Id);
            }*/

            return await query.ToArrayAsync();
        }

        public async Task<Audit> GetAuditForCriteriaPdfAsync(PdfFilter filter)
        {
            return await _context.Audit
                    .Include(audit => audit.Facility)
                        .ThenInclude(facility => facility.Organization)
                    .Include(audit => audit.FormVersion)
                        .ThenInclude(formVersion => formVersion.Form)
                    .Where(audit =>
                    audit.FacilityId == filter.FacilityId &&
                    audit.FormVersion.FormId == filter.FormId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Audit>> GetAuditsByUserTimeAndShift(int organizationId, int userId, DateTime startTime, DateTime endTime)
        {
            IQueryable<Audit> auditsQuery = _context.Audit
                .Include(audit => audit.SubmittedByUser)
                .Include(audit => audit.FormVersion)
                    .ThenInclude(form => form.Form)
                ;

            auditsQuery = auditsQuery.Where(audit =>
                audit.SubmittedDate >= startTime &&
                audit.SubmittedDate<=endTime &&
                audit.SubmittedByUserId==userId &&
                audit.Facility.OrganizationId == organizationId &&
                audit.Status == AuditStatus.WaitingForApproval
            ).Select(s => new Audit
            {
                Id = s.Id,
                FormVersion = s.FormVersion
            });
            List<Audit> audits = await auditsQuery.ToListAsync();
            return audits;
        }

        public async Task<List<Audit>> GetCompliantResidentsForCriteriaFilter(int organizationID, int[] facilityIDs, int[] formVersionIds, string fromDate, string toDate, string fromAuditDate, string toAuditDate, int questionId, int compliantType)
        {
            DateTime _from = DateTime.Parse(fromDate);
            DateTime _fromAuditDate = DateTime.Parse(fromAuditDate);
            DateTime _toAuditDate = DateTime.Parse(toAuditDate);

            var compliantValue = "yes";
            switch (compliantType) {
                case 1:
                    compliantValue = "yes";
                    break;
                case 2: compliantValue = "no";
                    break;
                case 3: compliantValue = "na";
                    break;
            }

            IQueryable<Audit> auditsQuery = _context.Audit
                .Include(audit => audit.Facility)
                    .ThenInclude(facility => facility.Organization)
                .Include(audit => audit.Values)
                    .ThenInclude(values => values.Column)
                .Include(audit => audit.AuditFieldValues)
                .Include(audit => audit.FormVersion)
                    .ThenInclude(formVersion => formVersion.Form)
                .Where(audit =>
                    facilityIDs.Contains(audit.FacilityId) &&
                    audit.State == AuditState.Active &&
                    audit.Status == AuditStatus.Submitted &&
                    audit.SubmittedDate >= _fromAuditDate &&
                    audit.SubmittedDate < _toAuditDate.AddDays(1) &&
                    audit.IncidentDateFrom >= _from &&
                    audit.Values.Where(value => value.TableColumnId == questionId && value.Value == compliantValue).Count() > 0 &&
                    formVersionIds.Contains(audit.FormVersionId)
                );

            if (toDate.Length>0)
            {
                DateTime _to = DateTime.Parse(toDate).AddDays(1);
                auditsQuery.Where(audit =>
                    audit.IncidentDateFrom < _to
                );
            }


            List<Audit> audits = await auditsQuery.ToListAsync();

            return audits;
        }

        public async Task<List<Audit>> GetSubmitedAuditsAsyncForFallAndWound(List<int> formIds, ReportFallFilterModel reportFallFilterModel)
        {

            DateTime _from = new DateTime(reportFallFilterModel.Year, reportFallFilterModel.Months[0], 1);
            DateTime _until = new DateTime(reportFallFilterModel.Year, reportFallFilterModel.Months[reportFallFilterModel.Months.Length - 1], DateTime.DaysInMonth(reportFallFilterModel.Year, reportFallFilterModel.Months[reportFallFilterModel.Months.Length - 1]));


            IQueryable<Audit> auditsQuery = _context.Audit
                .Include(audit => audit.SubmittedByUser)
                .Include(audit => audit.Facility)
                        .ThenInclude(facility => facility.Organization)
                .Include(audit => audit.Facility)
                        .ThenInclude(facility => facility.TimeZone)

                .Include(audit => audit.Values)
                .Include(audit => audit.AuditFieldValues)
                .Include(audit => audit.FormVersion)
                    .ThenInclude(formVersion => formVersion.Form)
                .Include(audit => audit.Settings)
                .Where(audit =>
                    audit.FacilityId == reportFallFilterModel.FacilityID &&
                    audit.State == AuditState.Active &&
                    audit.Status == AuditStatus.Submitted &&
                    formIds.Contains(audit.FormVersion.FormId)
                );

            List<Audit> audits = await auditsQuery.ToListAsync();
            var months = new List<KeyValuePair<DateTime, DateTime>>();

            foreach (var month in reportFallFilterModel.Months)
            {
                DateTime from = new DateTime(reportFallFilterModel.Year, month, 1);
                DateTime until = new DateTime(reportFallFilterModel.Year, month, DateTime.DaysInMonth(reportFallFilterModel.Year, month));
                months.Add(new KeyValuePair<DateTime, DateTime>(from, until));
            }

            audits = audits.FindAll(audit => months.Any(m => audit.IncidentDateFrom >= m.Key && audit.IncidentDateFrom <= m.Value));

            return audits;
        }

        public async Task<List<Audit>> GetAuditsOfFacility(int facilityId)
        {
            IQueryable<Audit> auditsQuery = _context.Audit
                .Include(audit => audit.Facility)
                .Where(audit => audit.FacilityId == facilityId && audit.Status == AuditStatus.Submitted && audit.ResidentName!=null);
            List<Audit> audits = await auditsQuery.ToListAsync();
            return audits;
        }

        public async Task<Audit> GetAuditForDuplicateAsync(int auditId)
        {
            return await _context.Audit
                .Include(audit => audit.Values)
                .Include(audit => audit.AuditFieldValues)
                .Include(audit => audit.Settings)

                .FirstOrDefaultAsync(audit => audit.Id == auditId);
        }

        public async Task<IReadOnlyCollection<AuditKPIQueryModel>> GetAuditKPIsAsync(DashboardFilter filter)
        {
            return await _context.Audit
                .Where(BuildDashboardFiltersCriteria(filter))
                .GroupBy(audit => new { audit.Facility.Organization.Id, audit.Facility.Organization.Name, audit.Status })
                .Select(auditGroup => new AuditKPIQueryModel()
                {
                    OrganizationId = auditGroup.Key.Id,
                    OrganizationName = auditGroup.Key.Name,
                    AuditStatus = auditGroup.Key.Status,
                    AuditCount = auditGroup.Count()
                })
                .ToArrayAsync();
        }

        public async Task<int> GetAuditDueDateCountAsync(DashboardFilter filter, Expression<Func<Audit, bool>> dueDatePredicate)
        {
            return await _context.Audit
                .Where(BuildDashboardFiltersCriteria(filter))
                .Where(dueDatePredicate)
                .CountAsync();
        }

        private IQueryable<Audit> GetAuditQuery()
        {
            return _context.Audit
                .Include(audit => audit.SubmittedByUser)
                .Include(audit => audit.Facility)
                        .ThenInclude(facility => facility.Organization)
                .Include(audit => audit.FormVersion)
                    .ThenInclude(formVersion => formVersion.Form)
                        .ThenInclude(form => form.AuditType)
                .Include(audit => audit.DeletedByUser)
                .Include(audit => audit.AuditStatusHistory)
                .Include(audit => audit.PortalReport)            
                .AsNoTracking();

        }

        private IQueryable<Audit> GetAuditDetailsQuery()
        {
            return _context.Audit
                .Include(audit => audit.SubmittedByUser)
                .Include(audit => audit.Facility)
                        .ThenInclude(facility => facility.Organization)
                .Include(audit => audit.Facility)
                        .ThenInclude(facility => facility.TimeZone)
                .Include(audit => audit.HighAlertAuditValues)
                .Include(audit => audit.FormVersion)
                    .ThenInclude(formVersion => formVersion.Form)
                        .ThenInclude(form => form.AuditType)

                .Include(audit => audit.Settings);
        }

        private Expression<Func<Audit, bool>> BuildOrganizationCriteria(ICollection<int> organizationIds)
        {
            if(organizationIds.Any())
            {
                return i => organizationIds.Contains(i.Facility.OrganizationId);
            }

            return i => true;
        }

        private Expression<Func<Audit, bool>> BuildFiltersCriteria(AuditFilter filter, AuditFilterColumn? column = null)
        {
            Expression<Func<Audit, bool>> auditExpr = PredicateBuilder.True<Audit>();

            if(filter == null)
            {
                return auditExpr;
            }

            auditExpr = auditExpr
                .AndIf(GetOrganizationExpression(filter), column != AuditFilterColumn.OrganizationName && filter.Organization != null && filter.Organization.Any())
                .AndIf(GetFacilityExpression(filter), column != AuditFilterColumn.FacilityName && filter.Facility != null && filter.Facility.Any())
                .AndIf(GetFormExpression(filter), column != AuditFilterColumn.FormName && filter.Form != null && filter.Form.Any())
                .AndIf(GetAuditTypeExpression(filter), column != AuditFilterColumn.AuditType && filter.AuditType != null && filter.AuditType.Any())
                .AndIf(GetSubmittedByExpression(filter), column != AuditFilterColumn.AuditorName && filter.Auditor != null && filter.Auditor.Any())
                .AndIf(GetUnitExpression(filter), column != AuditFilterColumn.Unit && filter.Unit != null && filter.Unit.Any())
                .AndIf(GetRoomExpression(filter), column != AuditFilterColumn.Room && filter.Room != null && filter.Room.Any())
                .AndIf(GetResidentExpression(filter), column != AuditFilterColumn.Resident && filter.Resident != null && filter.Resident.Any())
                .AndIf(GetStatusExpression(filter), column != AuditFilterColumn.Status && filter.Status != null && filter.Status.Any())
                .AndIf(GetDeletedByUserExpression(filter), column != AuditFilterColumn.DeletedByUser && filter.DeletedByUser != null && filter.DeletedByUser.Any());

            if (filter.AuditDate != null)
            {
                auditExpr = auditExpr.And(GetAuditDateExpression(filter));
            }

            if (filter.IncidentDateFrom != null)
            {
                auditExpr = auditExpr.And(GetIncidentDateFromExpression(filter));
            }

            if (filter.IncidentDateTo != null)
            {
                auditExpr = auditExpr.And(GetIncidentDateToExpression(filter));
            }

            if (filter.Compliance != null)
            {
                auditExpr = auditExpr.And(GetComplianceExpression(filter));
            }

            if (filter.LastDeletedDate != null)
            {
                auditExpr = auditExpr.And(GetLastDeletedDateExpression(filter));
            }

            return auditExpr;
        }

        private Expression<Func<Audit, bool>> GetOrganizationExpression(AuditFilter filter)
        {
            return i => filter.Organization.Select(option => option.Id).Contains(i.Facility.Organization.Id);
        }

        private Expression<Func<Audit, bool>> GetFacilityExpression(AuditFilter filter)
        {
            return i => filter.Facility.Select(option => option.Id).Contains(i.Facility.Id);
        }

        private Expression<Func<Audit, bool>> GetFormExpression(AuditFilter filter)
        {
            return i => filter.Form.Select(option => option.Id).Contains(i.FormVersion.Form.Id);
        }

        private Expression<Func<Audit, bool>> GetAuditTypeExpression(AuditFilter filter)
        {
            return i => filter.AuditType.Select(option => option.Id).Contains(i.FormVersion.Form.AuditType.Id);
        }

        private Expression<Func<Audit, bool>> GetSubmittedByExpression(AuditFilter filter)
        {
            return i => filter.Auditor.Select(option => option.Id).Contains(i.SubmittedByUser.Id);
        }

        private Expression<Func<Audit, bool>> GetUnitExpression(AuditFilter filter)
        {
            return i => filter.Unit.Select(option => option.Value).Contains(i.Unit);
        }

        private Expression<Func<Audit, bool>> GetRoomExpression(AuditFilter filter)
        {
            return i => filter.Room.Select(option => option.Value).Contains(i.Room);
        }

        private Expression<Func<Audit, bool>> GetResidentExpression(AuditFilter filter)
        {
            return i => filter.Resident.Select(option => option.Value).Contains(i.ResidentName);
        }

        private Expression<Func<Audit, bool>> GetComplianceExpression(AuditFilter filter)
        {
            return FilterHelper.GetFilterExpression<Audit, double>(
                nameof(Audit.TotalCompliance),
                filter.Compliance.FirstCondition,
                filter.Compliance.SecondCondition,
                filter.Compliance.Operator);
        }

        private Expression<Func<Audit, bool>> GetStatusExpression(AuditFilter filter)
        {
            return i => filter.Status.Select(option => (AuditStatus)option.Id).Contains(i.Status);
        }

        private Expression<Func<Audit, bool>> GetAuditDateExpression(AuditFilter filter)
        {
            return FilterHelper.GetFilterExpression<Audit, DateTime>(
                nameof(Audit.SubmittedDate),
                filter.AuditDate.FirstCondition,
                filter.AuditDate.SecondCondition,
                filter.AuditDate.Operator);
        }

        private Expression<Func<Audit, bool>> GetIncidentDateFromExpression(AuditFilter filter)
        {
            return FilterHelper.GetFilterExpression<Audit, DateTime>(
                nameof(Audit.IncidentDateFrom),
                filter.IncidentDateFrom.FirstCondition,
                filter.IncidentDateFrom.SecondCondition,
                filter.IncidentDateFrom.Operator);
        }

        private Expression<Func<Audit, bool>> GetIncidentDateToExpression(AuditFilter filter)
        {
            return FilterHelper.GetFilterExpression<Audit, DateTime>(
                nameof(Audit.IncidentDateTo),
                filter.IncidentDateTo.FirstCondition,
                filter.IncidentDateTo.SecondCondition,
                filter.IncidentDateTo.Operator);
        }

        private Expression<Func<Audit, bool>> GetLastDeletedDateExpression(AuditFilter filter)
        {
            return FilterHelper.GetFilterExpression<Audit, DateTime>(
                nameof(Audit.LastDeletedDate),
                filter.LastDeletedDate.FirstCondition,
                filter.LastDeletedDate.SecondCondition,
                filter.LastDeletedDate.Operator);
        }

        private Expression<Func<Audit, bool>> GetDeletedByUserExpression(AuditFilter filter)
        {
            return i => filter.DeletedByUser.Select(option => option.Id).Contains(i.DeletedByUser.Id);
        }

        private Expression<Func<Audit, bool>> BuildDashboardFiltersCriteria(DashboardFilter filter)
        {
            Expression<Func<Audit, bool>> auditExpr = i => i.State == AuditState.Active;

            if (filter == null)
            {
                return auditExpr;
            }

            auditExpr = auditExpr
                .AndIf(GetDashboardOrganizationExpression(filter), filter.Organizations.Count > 0)
                .AndIf(GetDashboardFacilityExpression(filter), filter.Facilities.Count > 0)
                .AndIf(GetDashboardFormExpression(filter), filter.Forms.Count > 0)
                .AndIf(GetDashboardDueDateTypeExpression(filter), filter.DueDateType > 0);

            if (filter.TimeFrame != null)
            {
                auditExpr = auditExpr.And(GetTimeFrameExpression(filter));
            }

            return auditExpr;
        }

        private Expression<Func<Audit, bool>> GetDashboardOrganizationExpression(DashboardFilter filter)
        {
            return i => filter.Organizations.Contains(i.Facility.OrganizationId);
        }

        private Expression<Func<Audit, bool>> GetDashboardFacilityExpression(DashboardFilter filter)
        {
            return i => filter.Facilities.Contains(i.FacilityId);
        }

        private Expression<Func<Audit, bool>> GetDashboardFormExpression(DashboardFilter filter)
        {
            return i => filter.Forms.Contains(i.FormVersion.FormId);
        }

        private Expression<Func<Audit, bool>> GetTimeFrameExpression(DashboardFilter filter)
        {
            return FilterHelper.GetFilterExpression<Audit, DateTime>(
                nameof(Audit.SubmittedDate),
                filter.TimeFrame.FirstCondition,
                filter.TimeFrame.SecondCondition,
                filter.TimeFrame.Operator);
        }

        private Expression<Func<Audit, bool>> GetDashboardDueDateTypeExpression(DashboardFilter filter)
        {
            switch(filter.DueDateType)
            {
                case DueDateType.Today:
                    return i => i.DueDate >= filter.CurrentClientDate && i.DueDate < filter.CurrentClientDate.AddDays(1);
                case DueDateType.Later:
                    return i => i.DueDate >= filter.CurrentClientDate.AddDays(1);
                default:
                    return i => true;

            }
        }
        private IQueryable<Audit> GetSubmittedAuditsPerOrganization(int organizationId)
        {
            var query = _context.Audit
                 .Include(audit => audit.Facility)
                         .ThenInclude(facility => facility.Organization)
                 .Include(audit => audit.FormVersion)
                     .ThenInclude(formVersion => formVersion.Form);
            return query.Where(x => x.State == AuditState.Active && x.Status == AuditStatus.Submitted && x.Facility.OrganizationId == organizationId);
        }

        private Expression<Func<Audit, bool>> BuildSummaryPerFacilityFilterException(AuditorProductivitySummaryPerFacilityFilter filter)
        {

            Expression<Func<Audit, bool>> auditExpr = PredicateBuilder.True<Audit>();

            if (filter == null)
            {
                return auditExpr;
            }
            auditExpr = auditExpr
                .AndIf(GetFacilityExpression(filter.Facilities), filter.Facilities != null && filter.Facilities.Any());
             
            if (filter.DateProcessed != null)
            {
                auditExpr = auditExpr.And(GetAuditDateExpression(filter.DateProcessed));
            }
            return auditExpr;
        }

        private Expression<Func<Audit, bool>> GetAuditDateExpression(DateFilterModel AuditDateFilter)
        {
            return FilterHelper.GetFilterExpression<Audit, DateTime>(
                nameof(Audit.SubmittedDate),
                AuditDateFilter.FirstCondition,
                AuditDateFilter.SecondCondition,
                AuditDateFilter.Operator);
        }

        private Expression<Func<Audit, bool>> GetFacilityExpression(IReadOnlyCollection<FilterOption> filterFacilities)
        {
            return i => filterFacilities.Select(option => option.Id).Contains(i.Facility.Id);
        }
        public async Task<List<Audit>> GetSubmittedAuditsPerDateAndOrganization(AuditorProductivitySummaryPerFacilityFilter filter)
        {
            var query = GetSubmittedAuditsPerOrganization(filter.Organization.Id.GetValueOrDefault())

            .Where(BuildSummaryPerFacilityFilterException(filter)).AsNoTracking();

            var audits = await  query.ToListAsync();
            return audits;
        }
    }
}