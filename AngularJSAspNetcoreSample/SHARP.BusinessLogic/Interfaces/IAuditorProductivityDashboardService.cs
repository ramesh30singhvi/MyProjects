using SHARP.BusinessLogic.DTO.AuditorProductivityDashboard;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic.Interfaces
{
    public interface IAuditorProductivityDashboardService
    {
        Task<IEnumerable<AuditorProductivityInputDto>> GetAuditorProductivityInputAsync(AuditorProductivityInputFilter filter);
        Task<IReadOnlyCollection<FilterOption>> GetInputFilterColumnSourceDataAsync(AuditorProductivityInputFilterColumnSource<AuditorProductivityInputFilterColumn> columnFilter);

        Task<IEnumerable<AuditorProductivityAHTPerAuditTypeDto>> GetAuditorProductivityAHTPerAuditTypeAsync(AuditorProductivityAHTPerAuditTypeFilter filter);
        Task<IReadOnlyCollection<FilterOption>> GetAHTPerAuditTypeFilterColumnSourceDataAsync(AuditorProductivityAHTPerAuditTypeFilterColumnSource<AuditorProductivityAHTPerAuditTypeFilterColumn> columnFilter);

        Task<IEnumerable<AuditorProductivitySummaryPerAuditorDto>> GetAuditorProductivitySummaryPerAuditorAsync(AuditorProductivitySummaryPerAuditorFilter filter);
        Task<IReadOnlyCollection<FilterOption>> GetSummaryPerAuditorFilterColumnSourceDataAsync(AuditorProductivitySummaryPerAuditorFilterColumnSource<AuditorProductivitySummaryPerAuditorFilterColumn> columnFilter);
        Task<AuditorProductivitySummaryPerFacilityDto> GetAuditorProductivitySummaryPerFacilityAsync(AuditorProductivitySummaryPerFacilityFilter filter);
        Task<IList<string>> GetFormTags();
    }
}
