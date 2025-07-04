using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.DTO.Form;
using SHARP.BusinessLogic.DTO.Organization;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic.Interfaces
{
    public interface IOrganizationService
    {
        Task<List<OrganizationDto>> GetAsync();
        List<OrganizationDetailedDto> GetDetailed();
        Task<OrganizationDetailedDto> AddAsync(AddOrganizationDto addOrganization);
        bool OrganizationExists(string name);
        bool OrganizationExists(int id, string name);
        Task<bool> EditOrganizationAsync(EditOrganizationDto editOrganization);

        Task<IReadOnlyCollection<OptionDto>> GetOrganizationOptionsAsync();

        Task<IEnumerable<FormOrganizationDto>> GetOrganizationFormsAsync(FormFilter filter);

        Task<IReadOnlyCollection<FilterOption>> GetOrganizationFormFilterColumnSourceDataAsync(OrganizationFormFilterColumnSource<OrganizationFormFilterColumn> columnData);

        Task<bool> SetFormSettingAsync(FormSettingDto formSettingDto);

        Task<bool> DeleteOrganizationAsync(int id);

        Task<OptionDto>  GetOrganizationAsync(int id);

        Task<OrganizationDetailedDto> GetDetailedAsync(int id);
    }
}
