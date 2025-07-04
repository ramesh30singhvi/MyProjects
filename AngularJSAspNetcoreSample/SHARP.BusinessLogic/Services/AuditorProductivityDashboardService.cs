using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SHARP.BusinessLogic.DTO.AuditorProductivityDashboard;
using SHARP.BusinessLogic.DTO.Facility;
using SHARP.BusinessLogic.Helpers;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Enums;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.Common.Helpers;
using SHARP.DAL;
using SHARP.DAL.Models.QueryModels;
using SHARP.DAL.Models.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic.Services
{
    public class AuditorProductivityDashboardService : IAuditorProductivityDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IAuditService _auditService;
        private readonly IFacilityService _facilityService;

        public AuditorProductivityDashboardService(IUnitOfWork unitOfWork, IConfiguration configuration,
            IMapper mapper,IAuditService auditService, IFacilityService facilityService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
            _auditService = auditService;
            _facilityService = facilityService;
        }

        #region Input

        public async Task<IEnumerable<AuditorProductivityInputDto>> GetAuditorProductivityInputAsync(AuditorProductivityInputFilter filter)
        {
            try
            {
                Expression<Func<AuditorProductivityInputView, object>> orderBySelector =
                    OrderByHelper.GetOrderBySelector<AuditorProductivityInputFilterColumn, Expression<Func<AuditorProductivityInputView, object>>>(filter.OrderBy, GetInputColumnOrderSelector);

                var auditorProductivityInput = await _unitOfWork.AuditorProductivityDashboardRepository.GetAuditorProductivityInputAsync(filter, orderBySelector);

                foreach (var item in auditorProductivityInput)
                {
                    var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(item.User.TimeZone);
                    DateTime currentTime = TimeZoneInfo.ConvertTimeFromUtc(item.StartTime.GetValueOrDefault(), timeZoneInfo);

                    // Check if current time is in DST
                    bool isDST = timeZoneInfo.IsDaylightSavingTime(currentTime);

                    if (IsTimeBetween(item.CompletionTime, 7, 10))
                        item.Hour = "7:00 - 10:00";
                    else if (IsTimeBetween(item.CompletionTime, 10, 13))
                        item.Hour = "10:00 - 13:00";
                    else if (IsTimeBetween(item.CompletionTime, 13, 15))
                        item.Hour = "13:00 - 15:00";
                    else if (isDST && IsTimeBetween(item.CompletionTime, 15, 16))
                        item.Hour = "15:00 - 16:00";
                    else if (IsTimeBetween(item.CompletionTime, 15, 17))
                        item.Hour = "15:00 - 17:00";
                    else
                        item.Hour = "Overtime Hours";

                    item.OverTimeUsed = IsTimeOverDue(item.FinalAHT, item.TargetAHTPerResident.GetValueOrDefault(), item.NoOfResidents.GetValueOrDefault());
                    

                }
                var auditorProductivityInputDto = _mapper.Map<IEnumerable<AuditorProductivityInputDto>>(auditorProductivityInput);

                return auditorProductivityInputDto;


            }catch(Exception ex)
            {
                string s = ex.Message;
            }

            return null;
        }

        private bool IsTimeOverDue(string finalAHT, int targetAHt, int numberOfResidents)
        {
            if (targetAHt > 0 && numberOfResidents > 0)
            {
                var totalAHTTarget = targetAHt * numberOfResidents;
                if (DateTime.TryParse(finalAHT, out DateTime date))
                {

                    if (GetMinutes(date.TimeOfDay.TotalMinutes) > totalAHTTarget)
                        return true;
                }
                else if( Int32.TryParse(finalAHT, out int number) && number > 0)
                {

                    if(GetMinutes(number/60) > totalAHTTarget)
                        return true;
                }

            }

            return false;
        }
        private double GetMinutes(double value)
        {
            double integerPart = Math.Floor(value);
            double fractional = value - integerPart;

            if (fractional > 0.5)
                return integerPart;                    // floor
            else if (fractional < 0.5)
                return Math.Ceiling(value);            // ceiling
            else
                return value;                          // exactly x.5  (or use Math.Floor(value) if you want)
        }
        private Expression<Func<AuditorProductivityInputView, object>> GetInputColumnOrderSelector(AuditorProductivityInputFilterColumn columnName)
        {
            var columnSelectorMap = new Dictionary<AuditorProductivityInputFilterColumn, Expression<Func<AuditorProductivityInputView, object>>>
            {

                { AuditorProductivityInputFilterColumn.Id, i => i.Id },
                { AuditorProductivityInputFilterColumn.StartTime, i => i.StartTime },
                { AuditorProductivityInputFilterColumn.CompletionTime, i => i.CompletionTime },
                { AuditorProductivityInputFilterColumn.UserFullName, i => i.UserFullName },
                { AuditorProductivityInputFilterColumn.FacilityName, i => i.FacilityName },
                { AuditorProductivityInputFilterColumn.TypeOfAudit, i => i.TypeOfAudit },
                { AuditorProductivityInputFilterColumn.NoOfFilteredAuditsAllType, i => i.NoOfFilteredAuditsAllType },
                { AuditorProductivityInputFilterColumn.HandlingTime, i => i.HandlingTime },
                { AuditorProductivityInputFilterColumn.AHTPerAudit, i => i.AHTPerAudit },
                { AuditorProductivityInputFilterColumn.Hour, i => i.Hour },
                { AuditorProductivityInputFilterColumn.NoOfFilteredAudits, i => i.NoOfFilteredAudits },
                { AuditorProductivityInputFilterColumn.FinalAHT, i => i.FinalAHT },
                { AuditorProductivityInputFilterColumn.Month, i => i.Month },
                { AuditorProductivityInputFilterColumn.NoOfResidents, i => i.NoOfResidents }
            };

            if (!columnSelectorMap.TryGetValue(columnName, out var columnSelector))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return columnSelector;
        }

        public async Task<IReadOnlyCollection<FilterOption>> GetInputFilterColumnSourceDataAsync(AuditorProductivityInputFilterColumnSource<AuditorProductivityInputFilterColumn> columnFilter)
        {
            if (columnFilter.Column == AuditorProductivityInputFilterColumn.Undefined)
            {
                throw new InvalidOperationException("Column is invalid");
            }

            var queryRule = GetInputColumnQueryRule(columnFilter.Column);

            var columnValues = await _unitOfWork.AuditorProductivityDashboardRepository.GetDistinctInputColumnAsync(columnFilter, queryRule);

            return _mapper.Map<IReadOnlyCollection<FilterOption>>(columnValues.OrderBy(i => i.Value));
        }

        private ColumnOptionQueryRule<AuditorProductivityInputView, FilterOptionQueryModel> GetInputColumnQueryRule(AuditorProductivityInputFilterColumn columnName)
        {
            var columnQueryRuleMap = new Dictionary<AuditorProductivityInputFilterColumn, ColumnOptionQueryRule<AuditorProductivityInputView, FilterOptionQueryModel>>
            {
                {
                    AuditorProductivityInputFilterColumn.Id,
                    new ColumnOptionQueryRule<AuditorProductivityInputView, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Value = i.Id.ToString() }
                    }
                },
                {
                    AuditorProductivityInputFilterColumn.UserFullName,
                    new ColumnOptionQueryRule<AuditorProductivityInputView, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Id = i.User.Id, Value = i.User.FullName }
                    }
                },
                {
                    AuditorProductivityInputFilterColumn.FacilityName,
                    new ColumnOptionQueryRule<AuditorProductivityInputView, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Id = i.Facility.Id, Value = i.Facility.Name }
                    }
                },
                {
                    AuditorProductivityInputFilterColumn.TypeOfAudit,
                    new ColumnOptionQueryRule<AuditorProductivityInputView, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Id = i.AuditType.Id, Value = i.AuditType.Name }
                    }
                },
                {
                    AuditorProductivityInputFilterColumn.NoOfFilteredAuditsAllType,
                    new ColumnOptionQueryRule<AuditorProductivityInputView, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Value = i.NoOfFilteredAuditsAllType.ToString() }
                    }
                },
                {
                    AuditorProductivityInputFilterColumn.HandlingTime,
                    new ColumnOptionQueryRule<AuditorProductivityInputView, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Value = i.HandlingTime }
                    }
                },
                {
                    AuditorProductivityInputFilterColumn.AHTPerAudit,
                    new ColumnOptionQueryRule<AuditorProductivityInputView, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Value = i.AHTPerAudit }
                    }
                },
                {
                    AuditorProductivityInputFilterColumn.Hour,
                    new ColumnOptionQueryRule<AuditorProductivityInputView, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Value = i.Hour }
                    }
                },
                {
                    AuditorProductivityInputFilterColumn.NoOfFilteredAudits,
                    new ColumnOptionQueryRule<AuditorProductivityInputView, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Value = i.NoOfFilteredAudits.ToString() }
                    }
                },
                {
                    AuditorProductivityInputFilterColumn.FinalAHT,
                    new ColumnOptionQueryRule<AuditorProductivityInputView, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Value = i.FinalAHT }
                    }
                },
                {
                    AuditorProductivityInputFilterColumn.Month,
                    new ColumnOptionQueryRule<AuditorProductivityInputView, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Value = i.Month }
                    }
                },
                {
                    AuditorProductivityInputFilterColumn.NoOfResidents,
                    new ColumnOptionQueryRule<AuditorProductivityInputView, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Value = i.NoOfResidents.ToString() }
                    }
                },
            };

            if (!columnQueryRuleMap.TryGetValue(columnName, out var queryRule))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return queryRule;
        }
        #endregion

        #region AHT Per AuditType

        public async Task<IEnumerable<AuditorProductivityAHTPerAuditTypeDto>> GetAuditorProductivityAHTPerAuditTypeAsync(AuditorProductivityAHTPerAuditTypeFilter filter)
        {

            var result = new List<AuditorProductivityAHTPerAuditTypeDto>();

            try
            {
                Expression<Func<AuditorProductivityAHTPerAuditTypeView, object>> orderBySelector =
                    OrderByHelper.GetOrderBySelector<AuditorProductivityAHTPerAuditTypeFilterColumn, Expression<Func<AuditorProductivityAHTPerAuditTypeView, object>>>(filter.OrderBy, GetAHTPerAuditTypeColumnOrderSelector);

                var auditorProductivityAHTPerAuditType = await _unitOfWork.AuditorProductivityDashboardRepository.GetAuditorProductivityAHTPerAuditTypeAsync(filter, orderBySelector);

                var auditorProductivityAHTPerAuditTypeDto = _mapper.Map<IEnumerable<AuditorProductivityAHTPerAuditTypeDto>>(auditorProductivityAHTPerAuditType);

                //Modify data
                var userGroups = auditorProductivityAHTPerAuditTypeDto
                                .Where(v => v.StartTime.HasValue && v.UserId.HasValue && v.AuditTypeId.HasValue)
                                .GroupBy(v => v.UserId.Value);

                foreach (var userGroup in userGroups)
                {
                    // Add total row for the user
                    var userTotalDto = new AuditorProductivityAHTPerAuditTypeDto
                    {
                        UserId = userGroup.Key,
                        UserFullName = userGroup.First().UserFullName,
                        FacilityId = null,
                        FacilityName = "",
                        AuditTypeId = null,
                        TypeOfAudit = "",
                        OverTimeUsed = null,
                        FinalAHT = (int?)userGroup.Sum(s => s.FinalAHT),
                        NoOfFilteredAudits = userGroup.Sum(s => s.NoOfFilteredAudits),
                    };
                    result.Add(userTotalDto);

                    // Group by Audit Type, Facility
                    var auditTypeGroups = userGroup
                                        .GroupBy(v => new { v.FacilityId, v.FacilityName, v.AuditTypeId, v.TypeOfAudit })
                                        .ToList();

                    foreach (var auditTypeGroup in auditTypeGroups)
                    {

                        var dto = new AuditorProductivityAHTPerAuditTypeDto
                        {
                            UserId = null,
                            UserFullName = "",
                            TargetAHTPerResident = auditTypeGroup.FirstOrDefault()?.TargetAHTPerResident,
                            FacilityId = auditTypeGroup.Key.FacilityId.Value,
                            FacilityName = auditTypeGroup.Key.FacilityName,
                            AuditTypeId = auditTypeGroup.Key.AuditTypeId.Value,
                            TypeOfAudit = auditTypeGroup.Key.TypeOfAudit,
                            NoOfResidents = auditTypeGroup.FirstOrDefault()?.NoOfResidents,
                            FinalAHT = (int?)auditTypeGroup.Sum(s => s.FinalAHT),
                            NoOfFilteredAudits = auditTypeGroup.Sum(s => s.NoOfFilteredAudits),
                            
                        };
                        dto.AHTPerResident = dto.FinalAHT / dto.NoOfResidents;
                        dto.OverTimeUsed = IsTimeOverDue(dto.FinalAHT.GetValueOrDefault().ToString(), dto.TargetAHTPerResident.GetValueOrDefault(), dto.NoOfResidents.GetValueOrDefault());
                        result.Add(dto);
                    }
                }

            }catch(Exception ex)
            {

            }

            return result;
        }

        private Expression<Func<AuditorProductivityAHTPerAuditTypeView, object>> GetAHTPerAuditTypeColumnOrderSelector(AuditorProductivityAHTPerAuditTypeFilterColumn columnName)
        {
            var columnSelectorMap = new Dictionary<AuditorProductivityAHTPerAuditTypeFilterColumn, Expression<Func<AuditorProductivityAHTPerAuditTypeView, object>>>
            {
                { AuditorProductivityAHTPerAuditTypeFilterColumn.UserFullName, i => i.UserFullName },
                { AuditorProductivityAHTPerAuditTypeFilterColumn.FacilityName, i => i.FacilityName },
                { AuditorProductivityAHTPerAuditTypeFilterColumn.TypeOfAudit, i => i.TypeOfAudit },
            };

            if (!columnSelectorMap.TryGetValue(columnName, out var columnSelector))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return columnSelector;
        }

        public async Task<IReadOnlyCollection<FilterOption>> GetAHTPerAuditTypeFilterColumnSourceDataAsync(AuditorProductivityAHTPerAuditTypeFilterColumnSource<AuditorProductivityAHTPerAuditTypeFilterColumn> columnFilter)
        {
            if (columnFilter.Column == AuditorProductivityAHTPerAuditTypeFilterColumn.Undefined)
            {
                throw new InvalidOperationException("Column is invalid");
            }

            var queryRule = GetAHTPerAuditTypeColumnQueryRule(columnFilter.Column);

            var columnValues = await _unitOfWork.AuditorProductivityDashboardRepository.GetDistinctAHTPerAuditTypeColumnAsync(columnFilter, queryRule);

            return _mapper.Map<IReadOnlyCollection<FilterOption>>(columnValues.OrderBy(i => i.Value));
        }

        private ColumnOptionQueryRule<AuditorProductivityAHTPerAuditTypeView, FilterOptionQueryModel> GetAHTPerAuditTypeColumnQueryRule(AuditorProductivityAHTPerAuditTypeFilterColumn columnName)
        {
            var columnQueryRuleMap = new Dictionary<AuditorProductivityAHTPerAuditTypeFilterColumn, ColumnOptionQueryRule<AuditorProductivityAHTPerAuditTypeView, FilterOptionQueryModel>>
            {
                {
                    AuditorProductivityAHTPerAuditTypeFilterColumn.UserFullName,
                    new ColumnOptionQueryRule<AuditorProductivityAHTPerAuditTypeView, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Id = i.User.Id, Value = i.User.FullName }
                    }
                },
                {
                    AuditorProductivityAHTPerAuditTypeFilterColumn.FacilityName,
                    new ColumnOptionQueryRule<AuditorProductivityAHTPerAuditTypeView, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Id = i.Facility.Id, Value = i.Facility.Name }
                    }
                },
                {
                    AuditorProductivityAHTPerAuditTypeFilterColumn.TypeOfAudit,
                    new ColumnOptionQueryRule<AuditorProductivityAHTPerAuditTypeView, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Id = i.AuditType.Id, Value = i.AuditType.Name }
                    }
                },
            };

            if (!columnQueryRuleMap.TryGetValue(columnName, out var queryRule))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return queryRule;
        }

        #endregion

        #region Summary Per Auditor

        public async Task<IEnumerable<AuditorProductivitySummaryPerAuditorDto>> GetAuditorProductivitySummaryPerAuditorAsync(AuditorProductivitySummaryPerAuditorFilter filter)
        {
            Expression<Func<AuditorProductivitySummaryPerAuditorView, object>> orderBySelector =
                OrderByHelper.GetOrderBySelector<AuditorProductivitySummaryPerAuditorFilterColumn, Expression<Func<AuditorProductivitySummaryPerAuditorView, object>>>(filter.OrderBy, GetSummaryPerAuditorColumnOrderSelector);
            var result = new List<AuditorProductivitySummaryPerAuditorDto>();

            try
            {
                var auditorProductivitySummaryPerAuditor = await _unitOfWork.AuditorProductivityDashboardRepository.GetAuditorProductivitySummaryPerAuditorAsync(filter, orderBySelector);


                var auditorProductivitySummaryPerAuditorDto = _mapper.Map<IEnumerable<AuditorProductivitySummaryPerAuditorDto>>(auditorProductivitySummaryPerAuditor);

                //Modify data
                var userGroups = auditorProductivitySummaryPerAuditorDto
                                .Where(v => v.StartTime.HasValue && v.UserId.HasValue && v.AuditTypeId.HasValue)
                                .GroupBy(v => v.UserId.Value);

                if(!userGroups.Any() )
                {
                    return result;
                }

   
                foreach (var userGroup in userGroups)
                {
                    var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(userGroup.First().UserTimezone);
                    DateTime currentTime = TimeZoneInfo.ConvertTimeFromUtc(userGroup.First().StartTime.GetValueOrDefault(), timeZoneInfo);

                    // Check if current time is in DST
                    bool isDST = timeZoneInfo.IsDaylightSavingTime(currentTime);

                    // Add total row for the user
                    var userTotalDto = new AuditorProductivitySummaryPerAuditorDto
                    {
                        UserId = userGroup.Key,
                        UserFullName = userGroup.First().UserFullName,
                        FacilityId = null,
                        FacilityName = "",
                        AuditTypeId = null,
                        TypeOfAudit = "",
                        OverTimeUsed = null,
                        IsDST = isDST,
                        // 7am-10am (7:00 - 10:00)
                        NoOfFilteredAudits8to10 = userGroup.Where(v => IsTimeBetween(v.StartTime, 7, 10)).Sum(v => v.NoOfFilteredAudits ?? 0),
                        UtilizedTime8to10 = userGroup.Where(v => IsTimeBetween(v.StartTime, 7, 10)).Sum(v => v.FinalAHT ?? 0),

                        // 10am-1pm (10:00 - 13:00)
                        NoOfFilteredAudits10to1 = userGroup.Where(v => IsTimeBetween(v.StartTime, 10, 13)).Sum(v => v.NoOfFilteredAudits ?? 0),
                        UtilizedTime10to1 = userGroup.Where(v => IsTimeBetween(v.StartTime, 10, 13)).Sum(v => v.FinalAHT ?? 0),

                        // 1pm-3pm (13:00 - 15:00)
                        NoOfFilteredAudits1to3 = userGroup.Where(v => IsTimeBetween(v.StartTime, 13, 15)).Sum(v => v.NoOfFilteredAudits ?? 0),
                        UtilizedTime1to3 = userGroup.Where(v => IsTimeBetween(v.StartTime, 13, 15)).Sum(v => v.FinalAHT ?? 0),

                        // Totals
                        TotalNoOfFilteredAudits = userGroup.Sum(v => v.NoOfFilteredAudits ?? 0),
                        TotalFinalAHT = userGroup.Sum(v => v.FinalAHT ?? 0)
                    };



                    if (userTotalDto.IsDST != null && userTotalDto.IsDST.GetValueOrDefault())
                    {
                        // 3pm-5pm (15:00 - 16:00)
                        userTotalDto.NoOfFilteredAudits3to5 = userGroup.Where(v => IsTimeBetween(v.StartTime, 15, 16)).Sum(v => v.NoOfFilteredAudits ?? 0);
                        userTotalDto.UtilizedTime3to5 = userGroup.Where(v => IsTimeBetween(v.StartTime, 15, 16)).Sum(v => v.FinalAHT ?? 0);


                        // Overtime (not between 8am-4pm)
                        userTotalDto.NoOfFilteredAuditsOvertimeHours = userGroup.Where(v => !IsTimeBetween(v.StartTime, 7, 16)).Sum(v => v.NoOfFilteredAudits ?? 0);
                        userTotalDto.UtilizedTimeOvertimeHours = userGroup.Where(v => !IsTimeBetween(v.StartTime,7, 16)).Sum(v => v.FinalAHT ?? 0);


                    }
                    else
                    {
                        // 3pm-5pm (15:00 - 17:00)
                        userTotalDto.NoOfFilteredAudits3to5 = userGroup.Where(v => IsTimeBetween(v.StartTime, 15, 17)).Sum(v => v.NoOfFilteredAudits ?? 0);
                        userTotalDto.UtilizedTime3to5 = userGroup.Where(v => IsTimeBetween(v.StartTime, 15, 17)).Sum(v => v.FinalAHT ?? 0);


                        // Overtime (not between 8am-5pm)
                        userTotalDto.NoOfFilteredAuditsOvertimeHours = userGroup.Where(v => !IsTimeBetween(v.StartTime, 7, 17)).Sum(v => v.NoOfFilteredAudits ?? 0);
                        userTotalDto.UtilizedTimeOvertimeHours = userGroup.Where(v => !IsTimeBetween(v.StartTime, 7, 17)).Sum(v => v.FinalAHT ?? 0);

                    }

                    result.Add(userTotalDto);

                    // Group by Audit Type, Facility
                    var auditTypeGroups = userGroup
                                        .GroupBy(v => new { v.FacilityId, v.FacilityName, v.AuditTypeId, v.TypeOfAudit })
                                        .ToList();

                    foreach (var auditTypeGroup in auditTypeGroups)
                    {
                        
                        var dto = new AuditorProductivitySummaryPerAuditorDto
                        {
                            UserId = null,
                            UserFullName = "",
                            FacilityId = auditTypeGroup.Key.FacilityId.Value,
                            FacilityName = auditTypeGroup.Key.FacilityName,
                            AuditTypeId = auditTypeGroup.Key.AuditTypeId.Value,
                            TypeOfAudit = auditTypeGroup.Key.TypeOfAudit,
                            NoOfResidents = auditTypeGroup.First().NoOfResidents,
                            TargetAHTPerResident = auditTypeGroup.First().TargetAHTPerResident,
                            IsDST = null,

                            // 8am-10am (7:00 - 10:00)
                            NoOfFilteredAudits8to10 = auditTypeGroup.Where(v => IsTimeBetween(v.StartTime, 7, 10)).Sum(v => v.NoOfFilteredAudits ?? 0),
                            UtilizedTime8to10 = auditTypeGroup.Where(v => IsTimeBetween(v.StartTime, 7, 10)).Sum(v => v.FinalAHT ?? 0),

                            // 10am-1pm (10:00 - 13:00)
                            NoOfFilteredAudits10to1 = auditTypeGroup.Where(v => IsTimeBetween(v.StartTime, 10, 13)).Sum(v => v.NoOfFilteredAudits ?? 0),
                            UtilizedTime10to1 = auditTypeGroup.Where(v => IsTimeBetween(v.StartTime, 10, 13)).Sum(v => v.FinalAHT ?? 0),

                            // 1pm-3pm (13:00 - 15:00)
                            NoOfFilteredAudits1to3 = auditTypeGroup.Where(v => IsTimeBetween(v.StartTime, 13, 15)).Sum(v => v.NoOfFilteredAudits ?? 0),
                            UtilizedTime1to3 = auditTypeGroup.Where(v => IsTimeBetween(v.StartTime, 13, 15)).Sum(v => v.FinalAHT ?? 0),

                            // Totals
                            TotalNoOfFilteredAudits = auditTypeGroup.Sum(v => v.NoOfFilteredAudits ?? 0),
                            TotalFinalAHT = auditTypeGroup.Sum(v => v.FinalAHT ?? 0)
                        };
                        if (userTotalDto.IsDST != null && userTotalDto.IsDST.GetValueOrDefault())
                        {
                            // 3pm-5pm (15:00 - 16:00)
                            dto.NoOfFilteredAudits3to5 = auditTypeGroup.Where(v => IsTimeBetween(v.StartTime, 15, 16)).Sum(v => v.NoOfFilteredAudits ?? 0);
                            dto.UtilizedTime3to5 = auditTypeGroup.Where(v => IsTimeBetween(v.StartTime, 15, 16)).Sum(v => v.FinalAHT ?? 0);


                            // Overtime (not between 8am-4pm)
                            dto.NoOfFilteredAuditsOvertimeHours = auditTypeGroup.Where(v => !IsTimeBetween(v.StartTime, 7, 16)).Sum(v => v.NoOfFilteredAudits ?? 0);
                            dto.UtilizedTimeOvertimeHours = auditTypeGroup.Where(v => !IsTimeBetween(v.StartTime, 7, 16)).Sum(v => v.FinalAHT ?? 0);
                        }
                        else
                        {
                            // 3pm-5pm (15:00 - 17:00)
                            dto.NoOfFilteredAudits3to5 = auditTypeGroup.Where(v => IsTimeBetween(v.StartTime, 15, 17)).Sum(v => v.NoOfFilteredAudits ?? 0);
                            dto.UtilizedTime3to5 = auditTypeGroup.Where(v => IsTimeBetween(v.StartTime, 15, 17)).Sum(v => v.FinalAHT ?? 0);



                            // Overtime (not between 8am-5pm)
                            dto.NoOfFilteredAuditsOvertimeHours = auditTypeGroup.Where(v => !IsTimeBetween(v.StartTime, 7, 17)).Sum(v => v.NoOfFilteredAudits ?? 0);
                            dto.UtilizedTimeOvertimeHours = auditTypeGroup.Where(v => !IsTimeBetween(v.StartTime, 7, 17)).Sum(v => v.FinalAHT ?? 0);

                        }


                        dto.OverTimeUsed = IsTimeOverDue(dto.TotalFinalAHT.GetValueOrDefault().ToString(), dto.TargetAHTPerResident.GetValueOrDefault(), dto.NoOfResidents.GetValueOrDefault());

                        result.Add(dto);
                    }
                }
            }catch(Exception ex)
            {
                string e = ex.Message;
            }

            return result;
        }

        private Expression<Func<AuditorProductivitySummaryPerAuditorView, object>> GetSummaryPerAuditorColumnOrderSelector(AuditorProductivitySummaryPerAuditorFilterColumn columnName)
        {
            var columnSelectorMap = new Dictionary<AuditorProductivitySummaryPerAuditorFilterColumn, Expression<Func<AuditorProductivitySummaryPerAuditorView, object>>>
            {
                { AuditorProductivitySummaryPerAuditorFilterColumn.UserFullName, i => i.UserFullName },
                { AuditorProductivitySummaryPerAuditorFilterColumn.FacilityName, i => i.FacilityName },
                { AuditorProductivitySummaryPerAuditorFilterColumn.TypeOfAudit, i => i.TypeOfAudit },
            };

            if (!columnSelectorMap.TryGetValue(columnName, out var columnSelector))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return columnSelector;
        }

        public async Task<IReadOnlyCollection<FilterOption>> GetSummaryPerAuditorFilterColumnSourceDataAsync(AuditorProductivitySummaryPerAuditorFilterColumnSource<AuditorProductivitySummaryPerAuditorFilterColumn> columnFilter)
        {
            if (columnFilter.Column == AuditorProductivitySummaryPerAuditorFilterColumn.Undefined)
            {
                throw new InvalidOperationException("Column is invalid");
            }

            var queryRule = GetSummaryPerAuditorColumnQueryRule(columnFilter.Column);

            var columnValues = await _unitOfWork.AuditorProductivityDashboardRepository.GetDistinctSummaryPerAuditorColumnAsync(columnFilter, queryRule);

            return _mapper.Map<IReadOnlyCollection<FilterOption>>(columnValues.OrderBy(i => i.Value));
        }

        private ColumnOptionQueryRule<AuditorProductivitySummaryPerAuditorView, FilterOptionQueryModel> GetSummaryPerAuditorColumnQueryRule(AuditorProductivitySummaryPerAuditorFilterColumn columnName)
        {
            var columnQueryRuleMap = new Dictionary<AuditorProductivitySummaryPerAuditorFilterColumn, ColumnOptionQueryRule<AuditorProductivitySummaryPerAuditorView, FilterOptionQueryModel>>
            {
                {
                    AuditorProductivitySummaryPerAuditorFilterColumn.UserFullName,
                    new ColumnOptionQueryRule<AuditorProductivitySummaryPerAuditorView, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Id = i.User.Id, Value = i.User.FullName }
                    }
                },
                {
                    AuditorProductivitySummaryPerAuditorFilterColumn.FacilityName,
                    new ColumnOptionQueryRule<AuditorProductivitySummaryPerAuditorView, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Id = i.Facility.Id, Value = i.Facility.Name }
                    }
                },
                {
                    AuditorProductivitySummaryPerAuditorFilterColumn.TypeOfAudit,
                    new ColumnOptionQueryRule<AuditorProductivitySummaryPerAuditorView, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Id = i.AuditType.Id, Value = i.AuditType.Name }
                    }
                },
            };

            if (!columnQueryRuleMap.TryGetValue(columnName, out var queryRule))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return queryRule;
        }

        private bool IsTimeBetween(DateTime? time, int startHour, int endHour)
        {
            if (!time.HasValue) return false;

            var timeOfDay = time.Value.TimeOfDay;
            var startTime = new TimeSpan(startHour, 0, 0);
            var endTime = new TimeSpan(endHour, 0, 0);

            return timeOfDay >= startTime && timeOfDay < endTime;
        }


        #endregion

        #region Summary Per Facility

        public async Task<AuditorProductivitySummaryPerFacilityDto> GetAuditorProductivitySummaryPerFacilityAsync(AuditorProductivitySummaryPerFacilityFilter filter)
        {
            FacilityOptionFilter facFilter = new FacilityOptionFilter();
            var organizations = new List<int>();
            organizations.Add(filter.Organization.Id.GetValueOrDefault());
            facFilter.OrganizationIds = organizations;
            var facilityIds = filter.Facilities.Select(f => f.Id.GetValueOrDefault()).ToList();
            facFilter.FacilityIds = facilityIds;
            //var dateFilterModel = JsonConvert.DeserializeObject<DateFilterModel>(filter.DateProcessed);
            facFilter.TakeCount = filter.TakeCount;
            facFilter.SkipCount = filter.SkipCount;

            var facilities = await _unitOfWork.FacilityRepository.GetFacilityOptionsAsync(facFilter);
           // var facilityIds = filter.Facilities.Select(f => f.Id).ToHashSet();
            if(facilityIds.Any()) 
                facilities = facilities.Where(f => facilityIds.Contains(f.Id)).ToList();

            var facilityIdsDto = _mapper.Map<IReadOnlyCollection<FacilityOptionDto>>(facilities);
            //return DateHelpers.ConvertToUtcDate(dateFilterModel, _userTimeZone);
            filter.Facilities = _mapper.Map< IReadOnlyCollection<FilterOption>>(facilityIdsDto);
            var result = new AuditorProductivitySummaryPerFacilityDto();
            var audits = await _unitOfWork.AuditRepository.GetSubmittedAuditsPerDateAndOrganization(filter);
            var auditsAIV1 = await _unitOfWork.AuditAIReportRepository.GetSubmittedAuditsPerDateAndOrganization(filter);
            var summaryPerFacilities = new List<AuditSummaryPerFacilityDto>();


            foreach (var facility in facilityIdsDto)
            {

                var auditSummaryPerFacilityDto = new AuditSummaryPerFacilityDto();
                auditSummaryPerFacilityDto.Facility = facility;
                var listOfFormCounter = new List< Tuple<string, int> >();

                foreach (var key in AuditorProductivityFormNameMap.Maro.Keys)
                {
                    int count = 0;
                    if (key == "24 Keyword AI V1")
                    {
                        count = auditsAIV1.Count(x =>  x.Facility.Id == facility.Id);
                    }
                    else
                    {
                        count = audits.Count(x => x.FormVersion.Form.Name.ToLower().Contains(key.ToLower()) && x.Facility.Id == facility.Id);
                    }
                    listOfFormCounter.Add(new Tuple<string, int>(key, count));
                    auditSummaryPerFacilityDto.TotalCount += count;
                }
                auditSummaryPerFacilityDto.FormProductivityResult = listOfFormCounter;
                summaryPerFacilities.Add(auditSummaryPerFacilityDto);
            }

            result.SummaryPerFacilities = summaryPerFacilities;

            return result;
        }

        public async Task<IList<string>> GetFormTags()
        {
            return AuditorProductivityFormNameMap.Maro.Keys.ToList();
        }

        #endregion

    }
}
