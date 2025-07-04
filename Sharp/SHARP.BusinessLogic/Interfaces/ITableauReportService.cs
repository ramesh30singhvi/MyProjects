using SHARP.BusinessLogic.DTO.Report;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic.Interfaces
{
    public interface ITableauReportService
    {
        Task<Uri> GetTableauReportUrlAsync();
        Task<List<ReportDto>> GetReportsAsync(ReportFilter filter);
        public Task<List<string>> GetFilterColumnSourceDataAsync(FilterColumnSource<ReportColumn> columnData);

        Task<ReportDto> GetReportsByIdAsync(int id);
    }
}
