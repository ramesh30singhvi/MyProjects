using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.DTO.Audit;
using SHARP.BusinessLogic.DTO.Portal;
using SHARP.BusinessLogic.DTO.Report;
using SHARP.BusinessLogic.Helpers;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Enums;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.Common.Models;
using SHARP.DAL;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic.Services
{
    public class HighAlertService : IHighAlertService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAuditService _auditService;
        private readonly IFacilityService _facilityService;

        public HighAlertService(
        IUnitOfWork unitOfWork,
        IMapper mapper,IAuditService auditService, IFacilityService facilityService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _auditService = auditService;
            _facilityService = facilityService;
        }

        public async Task<OptionDto[]> GetHighAlertPotentialAreas()
        {
            var areas = await _unitOfWork.HighAlertPotentialAreasRepository.GetAll().ToArrayAsync();

            var areasDto = _mapper.Map<OptionDto[]>(areas);

            return areasDto;
        }
        public async Task<OptionDto[]> GetHighAlertCategories()
        {
            var categories = await _unitOfWork.HighAlertCategoryRepository.GetAllActiveCategories();

            var categoriesDto = _mapper.Map<OptionDto[]>(categories);

            return categoriesDto;
        }

        public async Task<Tuple<IReadOnlyCollection<HighAlertValueDto>, int>> GetHighAlertsAsync(HighAlertPortalFilter filter)
        {
            Expression<Func<HighAlertAuditValue, object>> orderBySelector =
                   OrderByHelper.GetOrderBySelector<PortalHighAlertFilterColumn, Expression<Func<HighAlertAuditValue, object>>>(filter.OrderBy, GetColumnOrderSelector);

            //filter.UserOrganizationIds = await _userService.GetUserOrganizationIdsAsync();

            var tuple = await _unitOfWork.HighAlertAuditValueRepository.GetHighAlertsAsyncByPage(filter, orderBySelector);

            return new Tuple<IReadOnlyCollection<HighAlertValueDto>, int>(_mapper.Map<IReadOnlyCollection<HighAlertValueDto>>(tuple.Item1), tuple.Item2);
        }

        private Expression<Func<HighAlertAuditValue, object>> GetColumnOrderSelector(PortalHighAlertFilterColumn columnName)
        {
            var columnSelectorMap = new Dictionary<PortalHighAlertFilterColumn, Expression<Func<HighAlertAuditValue, object>>>
            {
                { PortalHighAlertFilterColumn.OrganizationName, i => i.AuditTableColumnValue.Audit.Facility.Organization.Name },
                { PortalHighAlertFilterColumn.FacilityName, i => i.AuditTableColumnValue.Audit.Facility.Name },
                { PortalHighAlertFilterColumn.HighAlertCategoryName, i => i.HighAlertCategory.Name },
            //    { PortalHighAlertFilterColumn.HighAlertStatusName, i => GetLatestHighAlertStatus(i) },
                { PortalHighAlertFilterColumn.Date, i => i.CreatedAt }
            };

            if (!columnSelectorMap.TryGetValue(columnName, out var columnSelector))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return columnSelector;
        }

        private string GetLatestHighAlertStatus(HighAlertAuditValue highAlert)
        {
            if(highAlert.HighAlertStatusHistory.ToArray().Any())
            {
                return highAlert.HighAlertStatusHistory.ToArray().Last().HighAlertStatus?.Name;
            }
            return string.Empty;
        }

        public async Task<OptionDto[]> GetHighAlertStatuses()
        {
            var statuses = _unitOfWork.HighAlertStatusRepository.GetAll();

            var statusesDto = _mapper.Map<OptionDto[]>(statuses);

            return statusesDto;
        }

        public async Task<HighAlertValueDto> SetHighAlertStatus(int highAlertId, OptionDto statusDto, string userNotes,string changeBy)
        {
            var highAlert = await _unitOfWork.HighAlertAuditValueRepository. GetHighAlertAuditValueAsync(highAlertId);

            if(highAlert == null)
                return null;

            var highAlertStatus = new HighAlertStatusHistory();
            highAlertStatus.HighAlertStatusId =  statusDto.Id;
            highAlertStatus.UserNotes = userNotes;
            highAlertStatus.CreatedAt = DateTime.UtcNow;
            highAlertStatus.ChangedBy = changeBy;
            highAlertStatus.HighAlertAuditValueId = highAlertId;

            await _unitOfWork.HighAlertStatusHistoryRepository.AddAsync(highAlertStatus);

            await _unitOfWork.SaveChangesAsync();

            var savedHighAlert = await _unitOfWork.HighAlertAuditValueRepository.GetHighAlertAuditValueAsync(highAlertId);

            return _mapper.Map<HighAlertValueDto>(savedHighAlert);
        }
        public async Task<byte[]> DownloadReportForHighAlertAsExcel(int id)
        {
            var highAlert = await _unitOfWork.HighAlertAuditValueRepository.GetAsync(id);
            if (highAlert == null)
                return null;
            return await _auditService.GetAuditExcelAsync(highAlert.AuditId);
        }
        public async Task<byte[]> DownloadReportForHighAlert(int id)
        {
            var highAlert = await _unitOfWork.HighAlertAuditValueRepository.GetAsync(id);
            if(highAlert == null)
                return null;
            return await _auditService.GetAuditPdfForHighAlertAsync(highAlert.AuditId);
        }

        public async Task<HighAlertStatisticDto> GetHighAlertStatistics(OptionDto facilityDto)
        {
            var facilityInfo = await _facilityService.GetFacilityDetailsAsync(facilityDto.Id);
            if(facilityInfo == null)
                return null;
            try
            {
                  var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(facilityInfo.TimeZone.OriginalTimeZoneName);
            }catch(Exception ex)
            {

            }
            var filter = new HighAlertPortalFilter();
            filter.Facility = new FilterOption();
            filter.Facility.Id = facilityDto.Id;
            filter.Date = new DateFilterModel();
            filter.Date.FirstCondition = new Condition<DateTime>();
            filter.Date.FirstCondition.From = DateTime.Now.AddDays(-4);
            filter.Date.FirstCondition.To = filter.Date.FirstCondition.From.AddDays(1);
            filter.Date.FirstCondition.Type = CompareType.InRange;
            //filter.AuditDate.FirstCondition.Type = CompareType.Equals;
            //filter.Date = new DateFilterModel();
            //filter.Date.FirstCondition.From = TimeZoneInfo.ConvertTimeToUtc(new DateTime(), timeZoneInfo);

            Expression<Func<HighAlertAuditValue, object>> orderBySelector =
                      OrderByHelper.GetOrderBySelector<PortalHighAlertFilterColumn, Expression<Func<HighAlertAuditValue, object>>>(filter.OrderBy, GetColumnOrderSelector);

    
            var tuple = await _unitOfWork.HighAlertAuditValueRepository.GetHighAlertsAsync(filter);
            if (tuple == null)
                return null;

            if (tuple.Item1 == null)
                return null;

            var highAlertStatisticDto = new HighAlertStatisticDto();
            highAlertStatisticDto.Total24Hours = tuple.Item2;
            highAlertStatisticDto.Closed24MHours = tuple.Item1.Count(x => x.HighAlertStatusHistory.Last()?.HighAlertStatus.Id == 2);
            var highAlertCategoryStatisticsFor24Hour = new List<HighAlertCategoryStatisticDto>();
            foreach ( var kvp in tuple.Item1)
            {
                if(highAlertCategoryStatisticsFor24Hour.Any(x => x.HighAlertCategory.Id == kvp.Id))
                {
                    var highAlertCategoryStatisticDto = highAlertCategoryStatisticsFor24Hour.FirstOrDefault(x => x.HighAlertCategory.Id == kvp.Id);
                    if(highAlertCategoryStatisticDto != null)
                    {
                        highAlertCategoryStatisticDto.CountUnClosedHighAlert += kvp.HighAlertStatusHistory.Last()?.Id == 1 ? 1 : 0;
                    }
                }
                else
                {
                    var highAlertCategoryStatisticDto = new HighAlertCategoryStatisticDto();
                    highAlertCategoryStatisticDto.HighAlertCategory = _mapper.Map<OptionDto>(kvp.HighAlertCategory);
                    highAlertCategoryStatisticDto.CountUnClosedHighAlert = kvp.HighAlertStatusHistory.Last()?.Id == 1 ? 1 : 0;
                    highAlertCategoryStatisticsFor24Hour.Add(highAlertCategoryStatisticDto);
                }

            }
            highAlertStatisticDto.HighAlertCategoryStatisticsFor24Hour = highAlertCategoryStatisticsFor24Hour;


            return highAlertStatisticDto;
        }

        public async Task<HighAlertCategoryPotentialAreaDto[]> GetHighAlertCategoriesWithPotentialArea()
        {
            var categories = await _unitOfWork.HighAlertCategoryRepository.GetAllActiveCategories();

            var categoriesWithAreaDto =  _mapper.Map<HighAlertCategoryPotentialAreaDto[]>(categories);

            return categoriesWithAreaDto;
        }

  
    }
}
