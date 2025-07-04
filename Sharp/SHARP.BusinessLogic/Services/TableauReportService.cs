using AutoMapper;
using Microsoft.Extensions.Configuration;
using SHARP.BusinessLogic.DTO.Report;
using SHARP.BusinessLogic.DTO.User;
using SHARP.BusinessLogic.Helpers;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic.Services
{
    public class TableauReportService : ITableauReportService
    {
        private readonly ITableauServerService _tableauServerService;

        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TableauReportService(
            ITableauServerService tableauServerService,
            IConfiguration configuration,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _tableauServerService = tableauServerService;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ReportDto>> GetReportsAsync(ReportFilter filter)
        {
            var orderBySelector = OrderByHelper.GetOrderBySelector<ReportColumn, Expression<Func<Report, object>>>(
                    filter.OrderBy,
                    GetOrderBySelector);
            var reports = await _unitOfWork.ReportRepository.GetAsync(filter, orderBySelector);
            var reportsDtos = _mapper.Map<IEnumerable<ReportDto>>(reports);

            return reportsDtos.ToList();
        }

        public async Task<ReportDto> GetReportsByIdAsync(int id)
        {
            var report = await _unitOfWork.ReportRepository.GetAsync(id);
            
            var reportsDto = _mapper.Map<ReportDto>(report);
            
            return reportsDto;
        }

        private Expression<Func<Report, object>> GetOrderBySelector(ReportColumn columnName)
        {
            var columnSelectorMap = new Dictionary<ReportColumn, Expression<Func<Report, object>>>
            {
                {
                    ReportColumn.Name,
                    i => i.Name
                }
            };

            if (!columnSelectorMap.TryGetValue(columnName, out var selector))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return selector;
        }

        public Task<List<string>> GetFilterColumnSourceDataAsync(FilterColumnSource<ReportColumn> columnData)
        {
            var queryRule = GetColumnQueryRule(columnData.Column);

            return _unitOfWork.ReportRepository.GetDistinctColumnAsync(queryRule);
        }

        private ColumnQueryRule<Report> GetColumnQueryRule(ReportColumn columnName)
        {
            var columnQueryRuleMap = new Dictionary<ReportColumn, ColumnQueryRule<Report>>
            {
                {
                    ReportColumn.Name,
                    new ColumnQueryRule<Report>
                    {
                        SingleSelector = i => i.Name
                    }
                }
            };

            if (!columnQueryRuleMap.TryGetValue(columnName, out var queryRule))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return queryRule;
        }

        private Expression<Func<Report, object>> GetColumnSelector(ReportColumn columnName)
        {
            var columnSelectorMap = new Dictionary<ReportColumn, Expression<Func<Report, object>>>
            {
                { ReportColumn.Name, i => i.Name }
            };

            if (!columnSelectorMap.TryGetValue(columnName, out var columnSelector))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return columnSelector;
        }
        public async Task<Uri> GetTableauReportUrlAsync()
        {
            string tableauTicket = await _tableauServerService.GetTableauAuthenticationTicketAsync();

            if (String.IsNullOrEmpty(tableauTicket))
            {
                return null;
            }

            string baseUri = _configuration["TableauServer:BaseUri"] ?? string.Empty; ;
            String tableauUrl = $"{baseUri}/trusted/{tableauTicket}/views/WeeklyreportACSPro_AuditCompliance/AuditCompliance?:iid=4";
            return new Uri(tableauUrl);
        }
    }
}
