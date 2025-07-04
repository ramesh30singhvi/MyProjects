using Microsoft.EntityFrameworkCore;
using SHARP.Common.Extensions;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Extensions;
using SHARP.DAL.Helpers;
using SHARP.DAL.Models;
using SHARP.DAL.Models.QueryModels;
using SHARP.DAL.Models.Views;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories
{
    public class AuditorProductivityDashboardRepository : GenericRepository<AuditorProductivityInputView>, IAuditorProductivityDashboardRepository
    {
        public AuditorProductivityDashboardRepository(SHARPContext context) : base(context)
        {
        }

        #region Input

        public async Task<AuditorProductivityInputView[]> GetAuditorProductivityInputAsync(AuditorProductivityInputFilter filter, Expression<Func<AuditorProductivityInputView, object>> orderBySelector)
        {
            var auditorProductivityInput = GetAuditorProductivityInputQuery(filter);

            return auditorProductivityInput
                .Where(BuildInputFiltersCriteria(filter))
                .QueryOrderBy(orderBySelector, filter.SortOrder)
                .ToArray();
                //.GetPagedAsync(filter.SkipCount, filter.TakeCount);
        }

        private IQueryable<AuditorProductivityInputView> GetAuditorProductivityInputQuery(AuditorProductivityInputFilter filter)
        {
            DateTime? fromDateStartTime = null;
            DateTime? toDateStartTime = null;
            if (filter.StartTime != null && filter.StartTime.FirstCondition != null)
            {
                fromDateStartTime = filter.StartTime.FirstCondition.From;
                toDateStartTime = filter.StartTime.FirstCondition.To;
            }

            DateTime? fromDateCompletionTime = null;
            DateTime? toDateCompletionTime = null;
            if (filter.CompletionTime != null && filter.CompletionTime.FirstCondition != null)
            {
                fromDateCompletionTime = filter.CompletionTime.FirstCondition.From;
                toDateCompletionTime = filter.CompletionTime.FirstCondition.To;
            }
            int? teamId = null;
            if (filter.Team != null && filter.Team.Id > 0)
                teamId = filter.Team.Id;


            return _context.AuditorProductivityInputView
                .FromSqlInterpolated($"EXEC GetAuditorProductivityInput {fromDateStartTime}, {toDateStartTime}, {fromDateCompletionTime}, {toDateCompletionTime} , {teamId} , {filter.SkipCount} , {filter.TakeCount}")
                .AsEnumerable()
                .Select(r => new AuditorProductivityInputView
                {
                    Id = r.Id,
                    StartTime = r.StartTime,
                    CompletionTime = r.CompletionTime,
                    UserId = r.UserId,
                    UserFullName = r.UserFullName,
                    FacilityId = r.FacilityId,
                    FacilityName = r.FacilityName,
                    AuditTypeId = r.AuditTypeId,
                    TypeOfAudit = r.TypeOfAudit,
                    NoOfFilteredAuditsAllType = r.NoOfFilteredAuditsAllType,
                    HandlingTime = r.HandlingTime,
                    AHTPerAudit = r.AHTPerAudit,
                    UserTimezone = r.UserTimezone,
                    Hour = string.Empty,
                    OverTimeUsed = false,
                    NoOfFilteredAudits = r.NoOfFilteredAudits,
                    FinalAHT = r.FinalAHT,
                    Month = r.Month,
                    NoOfResidents = r.NoOfResidents,
                    TargetAHTPerResident = r.TargetAHTPerResident,
                    User = new User
                    {
                        Id = (int)r.UserId,
                        FullName = r.UserFullName,
                        TimeZone = r.UserTimezone
                    },
                    Facility = new Facility
                    {
                        Id = (int)r.FacilityId,
                        Name = r.FacilityName,
                    },
                    AuditType = new Form
                    {
                        Id = (int)r.AuditTypeId,
                        Name = r.TypeOfAudit,
                    },
                })
                .AsQueryable()
                .AsNoTracking();
        }

        public async Task<IReadOnlyCollection<FilterOptionQueryModel>> GetDistinctInputColumnAsync(AuditorProductivityInputFilterColumnSource<AuditorProductivityInputFilterColumn> columnFilter, ColumnOptionQueryRule<AuditorProductivityInputView, FilterOptionQueryModel> queryRule)
        {
            var auditorProductivityInput = GetAuditorProductivityInputFilterQuery(columnFilter.AuditorProductivityInputFilter, columnFilter.Column);

            IQueryable<FilterOptionQueryModel> columnValues;
            if (queryRule.SingleSelector != null)
            {
                columnValues = auditorProductivityInput.Select(queryRule.SingleSelector);
            }
            else
            {
                columnValues = auditorProductivityInput.SelectMany(queryRule.ManySelector);
            }

            return columnValues
                .Distinct()
                .ToArray();
        }

        private IQueryable<AuditorProductivityInputView> GetAuditorProductivityInputFilterQuery(AuditorProductivityInputFilter filter, AuditorProductivityInputFilterColumn? column = null)
        {
            var auditorProductivityInput = GetAuditorProductivityInputQuery(filter)
                                            .Where(BuildInputFiltersCriteria(filter, column));

            return auditorProductivityInput;
        }

        private Expression<Func<AuditorProductivityInputView, bool>> BuildInputFiltersCriteria(AuditorProductivityInputFilter filter, AuditorProductivityInputFilterColumn? column = null)
        {
            Expression<Func<AuditorProductivityInputView, bool>> auditorProductivityInputExpr = PredicateBuilder.True<AuditorProductivityInputView>();

            if (filter == null)
            {
                return auditorProductivityInputExpr;
            }

            var expression = PredicateBuilder
            .True<AuditorProductivityInputView>()
                .AndIf(GetIdInputExpression(filter), column != AuditorProductivityInputFilterColumn.Id && filter.Id != null && filter.Id.Any())
                .AndIf(GetUserFullNameInputExpression(filter), column != AuditorProductivityInputFilterColumn.UserFullName && filter.UserFullName != null && filter.UserFullName.Any())
                .AndIf(GetFacilityInputExpression(filter), column != AuditorProductivityInputFilterColumn.FacilityName && filter.FacilityName != null && filter.FacilityName.Any());
               // .AndIf(GetTypeOfAuditInputExpression(filter), column != AuditorProductivityInputFilterColumn.TypeOfAudit && filter.TypeOfAudit != null && filter.TypeOfAudit.Any())
               // .AndIf(GetNoOfFilteredAuditsAllTypeInputExpression(filter), column != AuditorProductivityInputFilterColumn.NoOfFilteredAuditsAllType && filter.NoOfFilteredAuditsAllType != null && filter.NoOfFilteredAuditsAllType.Any())
               // .AndIf(GetHandlingTimeInputExpression(filter), column != AuditorProductivityInputFilterColumn.HandlingTime && filter.HandlingTime != null && filter.HandlingTime.Any())
               // .AndIf(GetAHTPerAuditInputExpression(filter), column != AuditorProductivityInputFilterColumn.HandlingTime && filter.HandlingTime != null && filter.HandlingTime.Any())
                //.AndIf(GetHourInputExpression(filter), column != AuditorProductivityInputFilterColumn.Hour && filter.Hour != null && filter.Hour.Any())
               // .AndIf(GetNoOfFilteredAuditsInputExpression(filter), column != AuditorProductivityInputFilterColumn.NoOfFilteredAudits && filter.NoOfFilteredAudits != null && filter.NoOfFilteredAudits.Any())
               // .AndIf(GetFinalAHTInputExpression(filter), column != AuditorProductivityInputFilterColumn.FinalAHT && filter.FinalAHT != null && filter.FinalAHT.Any())
               // .AndIf(GetMonthInputExpression(filter), column != AuditorProductivityInputFilterColumn.Month && filter.Month != null && filter.Month.Any())
               // .AndIf(GetNoOfResidentsInputExpression(filter), column != AuditorProductivityInputFilterColumn.NoOfResidents && filter.NoOfResidents != null && filter.NoOfResidents.Any());

            return expression;
        }

        private Expression<Func<AuditorProductivityInputView, bool>> GetIdInputExpression(AuditorProductivityInputFilter filter)
        {
            return i => filter.Id.Select(option => option.Value).Contains(i.Id.ToString());
        }

        private Expression<Func<AuditorProductivityInputView, bool>> GetUserFullNameInputExpression(AuditorProductivityInputFilter filter)
        {
            return i => filter.UserFullName.Select(option => option.Id).Contains(i.UserId);
        }

        private Expression<Func<AuditorProductivityInputView, bool>> GetFacilityInputExpression(AuditorProductivityInputFilter filter)
        {
            return i => filter.FacilityName.Select(option => option.Id).Contains(i.FacilityId);
        }

        private Expression<Func<AuditorProductivityInputView, bool>> GetTypeOfAuditInputExpression(AuditorProductivityInputFilter filter)
        {
            return i => filter.TypeOfAudit.Select(option => option.Id).Contains(i.AuditTypeId);
        }

        private Expression<Func<AuditorProductivityInputView, bool>> GetNoOfFilteredAuditsAllTypeInputExpression(AuditorProductivityInputFilter filter)
        {
            return i => filter.NoOfFilteredAuditsAllType.Select(option => option.Value).Contains(i.NoOfFilteredAuditsAllType.ToString());
        }
        
        private Expression<Func<AuditorProductivityInputView, bool>> GetHandlingTimeInputExpression(AuditorProductivityInputFilter filter)
        {
            return i => filter.HandlingTime.Select(option => option.Value).Contains(i.HandlingTime);
        }

        private Expression<Func<AuditorProductivityInputView, bool>> GetAHTPerAuditInputExpression(AuditorProductivityInputFilter filter)
        {
            return i => filter.AHTPerAudit.Select(option => option.Value).Contains(i.AHTPerAudit);
        }

        private Expression<Func<AuditorProductivityInputView, bool>> GetHourInputExpression(AuditorProductivityInputFilter filter)
        {
            return i => filter.Hour.Select(option => option.Value).Contains(i.Hour);
        }

        private Expression<Func<AuditorProductivityInputView, bool>> GetNoOfFilteredAuditsInputExpression(AuditorProductivityInputFilter filter)
        {
            return i => filter.NoOfFilteredAudits.Select(option => option.Value).Contains(i.NoOfFilteredAudits.ToString());
        }

        private Expression<Func<AuditorProductivityInputView, bool>> GetFinalAHTInputExpression(AuditorProductivityInputFilter filter)
        {
            return i => filter.FinalAHT.Select(option => option.Value).Contains(i.FinalAHT);
        }

        private Expression<Func<AuditorProductivityInputView, bool>> GetMonthInputExpression(AuditorProductivityInputFilter filter)
        {
            return i => filter.Month.Select(option => option.Value).Contains(i.Month);
        }

        private Expression<Func<AuditorProductivityInputView, bool>> GetNoOfResidentsInputExpression(AuditorProductivityInputFilter filter)
        {
            return i => filter.NoOfResidents.Select(option => option.Value).Contains(i.NoOfResidents.ToString());
        }

        #endregion

        #region AHT Per AuditType

        public async Task<AuditorProductivityAHTPerAuditTypeView[]> GetAuditorProductivityAHTPerAuditTypeAsync(AuditorProductivityAHTPerAuditTypeFilter filter, Expression<Func<AuditorProductivityAHTPerAuditTypeView, object>> orderBySelector)
        {
            var auditorProductivityAHTPerAuditType = GetAuditorProductivityAHTPerAuditTypeQuery(filter);

            var array =  auditorProductivityAHTPerAuditType
                .Where(BuildAHTPerAuditTypeFiltersCriteria(filter))
                .QueryOrderBy(orderBySelector, filter.SortOrder)
                .ToArray();

            return array;
        }

        private IQueryable<AuditorProductivityAHTPerAuditTypeView> GetAuditorProductivityAHTPerAuditTypeQuery(AuditorProductivityAHTPerAuditTypeFilter filter)
        {
            DateTime? fromDateStartTime = null;
            DateTime? toDateStartTime = null;
            if (filter.DateProcessed != null && filter.DateProcessed.FirstCondition != null)
            {
                fromDateStartTime = filter.DateProcessed.FirstCondition.From;
                toDateStartTime = filter.DateProcessed.FirstCondition.To;
            }
            int? teamId = null;
            if (filter.Team != null && filter.Team.Id > 0)
                teamId = filter.Team.Id;

            return _context.AuditorProductivityAHTPerAuditTypeView
                .FromSqlInterpolated($"EXEC GetAuditorProductivityAHTPerAuditType {fromDateStartTime}, {toDateStartTime}, {teamId}, {filter.SkipCount} , {filter.TakeCount}")
                .AsEnumerable()
                .Select(r => new AuditorProductivityAHTPerAuditTypeView
                {
                    StartTime = r.StartTime,
                    UserId = r.UserId,
                    UserFullName = r.UserFullName,
                    FacilityId = r.FacilityId,
                    FacilityName = r.FacilityName,
                    AuditTypeId = r.AuditTypeId,
                    TypeOfAudit = r.TypeOfAudit,
                    NoOfFilteredAudits = r.NoOfFilteredAudits,
                    FinalAHT = r.FinalAHT,
                    NoOfResidents = r.NoOfResidents,
                    TargetAHTPerResident = r.TargetAHTPerResident,
                    User = new User
                    {
                        Id = (int)r.UserId,
                        FullName = r.UserFullName,
                    },
                    Facility = new Facility
                    {
                        Id = (int)r.FacilityId,
                        Name = r.FacilityName,
                    },
                    AuditType = new Form
                    {
                        Id = (int)r.AuditTypeId,
                        Name = r.TypeOfAudit,
                    },
                })
                .AsQueryable()
                .AsNoTracking();
        }

        public async Task<IReadOnlyCollection<FilterOptionQueryModel>> GetDistinctAHTPerAuditTypeColumnAsync(AuditorProductivityAHTPerAuditTypeFilterColumnSource<AuditorProductivityAHTPerAuditTypeFilterColumn> columnFilter, ColumnOptionQueryRule<AuditorProductivityAHTPerAuditTypeView, FilterOptionQueryModel> queryRule)
        {
            var auditorProductivityAHTPerAuditType = GetAuditorProductivityAHTPerAuditTypeFilterQuery(columnFilter.AuditorProductivityAHTPerAuditTypeFilter, columnFilter.Column);

            IQueryable<FilterOptionQueryModel> columnValues;
            if (queryRule.SingleSelector != null)
            {
                columnValues = auditorProductivityAHTPerAuditType.Select(queryRule.SingleSelector);
            }
            else
            {
                columnValues = auditorProductivityAHTPerAuditType.SelectMany(queryRule.ManySelector);
            }

            return columnValues
                .Distinct()
                .ToArray();
        }

        private IQueryable<AuditorProductivityAHTPerAuditTypeView> GetAuditorProductivityAHTPerAuditTypeFilterQuery(AuditorProductivityAHTPerAuditTypeFilter filter, AuditorProductivityAHTPerAuditTypeFilterColumn? column = null)
        {
            var auditorProductivityAHTPerAuditType = GetAuditorProductivityAHTPerAuditTypeQuery(filter)
                                            .Where(BuildAHTPerAuditTypeFiltersCriteria(filter, column));

            return auditorProductivityAHTPerAuditType;
        }

        private Expression<Func<AuditorProductivityAHTPerAuditTypeView, bool>> BuildAHTPerAuditTypeFiltersCriteria(AuditorProductivityAHTPerAuditTypeFilter filter, AuditorProductivityAHTPerAuditTypeFilterColumn? column = null)
        {
            Expression<Func<AuditorProductivityAHTPerAuditTypeView, bool>> auditorProductivityAHTPerAuditTypeExpr = PredicateBuilder.True<AuditorProductivityAHTPerAuditTypeView>();

            if (filter == null)
            {
                return auditorProductivityAHTPerAuditTypeExpr;
            }

            var expression = PredicateBuilder
            .True<AuditorProductivityAHTPerAuditTypeView>()
                .AndIf(GetUserFullNameAHTPerAuditTypeExpression(filter), column != AuditorProductivityAHTPerAuditTypeFilterColumn.UserFullName && filter.UserFullName != null && filter.UserFullName.Any())
                .AndIf(GetFacilityAHTPerAuditTypeExpression(filter), column != AuditorProductivityAHTPerAuditTypeFilterColumn.FacilityName && filter.FacilityName != null && filter.FacilityName.Any())
                .AndIf(GetTypeOfAuditAHTPerAuditTypeExpression(filter), column != AuditorProductivityAHTPerAuditTypeFilterColumn.TypeOfAudit && filter.TypeOfAudit != null && filter.TypeOfAudit.Any());
                
            return expression;
        }

        private Expression<Func<AuditorProductivityAHTPerAuditTypeView, bool>> GetUserFullNameAHTPerAuditTypeExpression(AuditorProductivityAHTPerAuditTypeFilter filter)
        {
            return i => filter.UserFullName.Select(option => option.Id).Contains(i.UserId);
        }

        private Expression<Func<AuditorProductivityAHTPerAuditTypeView, bool>> GetFacilityAHTPerAuditTypeExpression(AuditorProductivityAHTPerAuditTypeFilter filter)
        {
            return i => filter.FacilityName.Select(option => option.Id).Contains(i.FacilityId);
        }

        private Expression<Func<AuditorProductivityAHTPerAuditTypeView, bool>> GetTypeOfAuditAHTPerAuditTypeExpression(AuditorProductivityAHTPerAuditTypeFilter filter)
        {
            return i => filter.TypeOfAudit.Select(option => option.Id).Contains(i.AuditTypeId);
        }

        #endregion

        #region Summary Per Auditor

        public async Task<AuditorProductivitySummaryPerAuditorView[]> GetAuditorProductivitySummaryPerAuditorAsync(AuditorProductivitySummaryPerAuditorFilter filter, Expression<Func<AuditorProductivitySummaryPerAuditorView, object>> orderBySelector)
        {
            var auditorProductivitySummaryPerAuditor = GetAuditorProductivitySummaryPerAuditorQuery(filter);

            return auditorProductivitySummaryPerAuditor
                .Where(BuildSummaryPerAuditorFiltersCriteria(filter))
                .QueryOrderBy(orderBySelector, filter.SortOrder)
                .ToArray();
        }

        private IQueryable<AuditorProductivitySummaryPerAuditorView> GetAuditorProductivitySummaryPerAuditorQuery(AuditorProductivitySummaryPerAuditorFilter filter)
        {
            DateTime? fromDateStartTime = null;
            DateTime? toDateStartTime = null;
            if (filter.DateProcessed != null && filter.DateProcessed.FirstCondition != null)
            {
                fromDateStartTime = filter.DateProcessed.FirstCondition.From;
                toDateStartTime = filter.DateProcessed.FirstCondition.To;
            }
            int? teamId = null;
            if (filter.Team != null && filter.Team.Id > 0)
                teamId = filter.Team.Id;

            return _context.AuditorProductivitySummaryPerAuditorView
                .FromSqlInterpolated($"EXEC GetAuditorProductivitySummaryPerAuditor {fromDateStartTime}, {toDateStartTime} , {teamId}")
                .AsEnumerable()
                .Select(r => new AuditorProductivitySummaryPerAuditorView
                {
                    StartTime = r.StartTime,
                    UserId = r.UserId,
                    UserTimezone = r.UserTimezone,
                    UserFullName = r.UserFullName,
                    FacilityId = r.FacilityId,
                    FacilityName = r.FacilityName,
                    AuditTypeId = r.AuditTypeId,
                    TypeOfAudit = r.TypeOfAudit,
                    FinalAHT = r.FinalAHT,
                    NoOfFilteredAudits = r.NoOfFilteredAudits,
                    NoOfResidents = r.NoOfResidents,                 
                    TargetAHTPerResident = r.TargetAHTPerResident,
                    User = new User
                    {
                        Id = (int)r.UserId,
                        FullName = r.UserFullName,
                        TimeZone = r.UserTimezone
                    },
                    Facility = new Facility
                    {
                        Id = (int)r.FacilityId,
                        Name = r.FacilityName,
                    },
                    AuditType = new Form
                    {
                        Id = (int)r.AuditTypeId,
                        Name = r.TypeOfAudit,
                    },
                })
                .AsQueryable()
                .AsNoTracking();
        }

        public async Task<IReadOnlyCollection<FilterOptionQueryModel>> GetDistinctSummaryPerAuditorColumnAsync(AuditorProductivitySummaryPerAuditorFilterColumnSource<AuditorProductivitySummaryPerAuditorFilterColumn> columnFilter, ColumnOptionQueryRule<AuditorProductivitySummaryPerAuditorView, FilterOptionQueryModel> queryRule)
        {
            var auditorProductivitySummaryPerAuditor = GetAuditorProductivitySummaryPerAuditorFilterQuery(columnFilter.AuditorProductivitySummaryPerAuditorFilter, columnFilter.Column);

            IQueryable<FilterOptionQueryModel> columnValues;
            if (queryRule.SingleSelector != null)
            {
                columnValues = auditorProductivitySummaryPerAuditor.Select(queryRule.SingleSelector);
            }
            else
            {
                columnValues = auditorProductivitySummaryPerAuditor.SelectMany(queryRule.ManySelector);
            }

            return columnValues
                .Distinct()
                .ToArray();
        }

        private IQueryable<AuditorProductivitySummaryPerAuditorView> GetAuditorProductivitySummaryPerAuditorFilterQuery(AuditorProductivitySummaryPerAuditorFilter filter, AuditorProductivitySummaryPerAuditorFilterColumn? column = null)
        {
            var auditorProductivitySummaryPerAuditor = GetAuditorProductivitySummaryPerAuditorQuery(filter)
                                            .Where(BuildSummaryPerAuditorFiltersCriteria(filter, column));

            return auditorProductivitySummaryPerAuditor;
        }

        private Expression<Func<AuditorProductivitySummaryPerAuditorView, bool>> BuildSummaryPerAuditorFiltersCriteria(AuditorProductivitySummaryPerAuditorFilter filter, AuditorProductivitySummaryPerAuditorFilterColumn? column = null)
        {
            Expression<Func<AuditorProductivitySummaryPerAuditorView, bool>> auditorProductivitySummaryPerAuditorExpr = PredicateBuilder.True<AuditorProductivitySummaryPerAuditorView>();

            if (filter == null)
            {
                return auditorProductivitySummaryPerAuditorExpr;
            }

            var expression = PredicateBuilder
            .True<AuditorProductivitySummaryPerAuditorView>()
                .AndIf(GetUserFullNameSummaryPerAuditorExpression(filter), column != AuditorProductivitySummaryPerAuditorFilterColumn.UserFullName && filter.UserFullName != null && filter.UserFullName.Any())
                .AndIf(GetFacilitySummaryPerAuditorExpression(filter), column != AuditorProductivitySummaryPerAuditorFilterColumn.FacilityName && filter.FacilityName != null && filter.FacilityName.Any())
                .AndIf(GetTypeOfAuditSummaryPerAuditorExpression(filter), column != AuditorProductivitySummaryPerAuditorFilterColumn.TypeOfAudit && filter.TypeOfAudit != null && filter.TypeOfAudit.Any());

            return expression;
        }

        private Expression<Func<AuditorProductivitySummaryPerAuditorView, bool>> GetUserFullNameSummaryPerAuditorExpression(AuditorProductivitySummaryPerAuditorFilter filter)
        {
            if (filter.UserFullName == null)
                return i => true;

            return i => filter.UserFullName.Select(option => option.Value).Contains(i.UserFullName);
        }

        private Expression<Func<AuditorProductivitySummaryPerAuditorView, bool>> GetFacilitySummaryPerAuditorExpression(AuditorProductivitySummaryPerAuditorFilter filter)
        {
            if (filter.FacilityName == null)
                return i => true;

            return i => filter.FacilityName.Select(option => option.Value).Contains(i.FacilityName);
        }

        private Expression<Func<AuditorProductivitySummaryPerAuditorView, bool>> GetTypeOfAuditSummaryPerAuditorExpression(AuditorProductivitySummaryPerAuditorFilter filter)
        {
            if (filter.TypeOfAudit == null)
                return i => true;
            return i => filter.TypeOfAudit.Select(option => option.Id).Contains(i.AuditTypeId);
        }

        #endregion
    }
}
