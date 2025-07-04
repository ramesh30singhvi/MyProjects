using SHARP.BusinessLogic.DTO.Facility;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic.Interfaces
{
    public interface IFacilityService
    {
        Task<IEnumerable<FacilityDto>> GetFacilitiesAsync(FacilityFilter filter);

        Task<IReadOnlyCollection<FacilityOptionDto>> GetFacilityOptionsAsync(int organizationId);

        Task<IReadOnlyCollection<FacilityOptionDto>> GetFacilityOptionsAsync(FacilityOptionFilter filter);

        Task<IReadOnlyCollection<TimeZoneOptionDto>> GetTimeZoneOptionsAsync();

        Task<FacilityDetailsDto> GetFacilityDetailsAsync(int id);

        Task<FacilityDetailsDto> GetFacilityByNameAsync(string name,int organizationiD);

        Task<bool> AddFacilityAsync(AddFacilityDto addFacilityDto);

        Task<bool> EditFacilityAsync(EditFacilityDto editFacilityDto);

        Task<bool> IsFacilityNameAlreadyExist(string name, int? organizationId, int? facilityId = null);

        Task<IReadOnlyCollection<FilterOption>> GetFilterColumnSourceDataAsync(FacilityFilterColumnSource<FacilityFilterColumn> columnData);

        Task<bool> DeleteFacilityAsync(int id);
        Task<bool> AddFacilityRecipientsAsync(AddEmailRecipientsDto addEmailRecipientsFacilityDto);
        Task ExportFacilityRecipientsToAnotherDB();
        Task<string[]> GetEmailRecipients(int facilityID);

        Task<string> GetProviderInformationByFacilityAsync(string facilityName);
        Task<string> GetHealthDeficienciesByFacilityAsync(string facilityName);
        Task<string> GetMDSQualityMeasuresDataByFacilityAsync(string facilityName);
        Task<string> GetMedicareClaimsQualityMeasuresDataByFacilityAsync(string facilityName);

        Task<AuditSummaryDto[]> GetAuditSummaryAsync(int facilityId, string fromDate, string toDate);

    }
}
