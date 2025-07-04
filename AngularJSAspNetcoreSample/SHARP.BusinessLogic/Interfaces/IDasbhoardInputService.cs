using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SHARP.BusinessLogic.DTO.Dashboard;
using SHARP.DAL.Models;

namespace SHARP.BusinessLogic.Interfaces
{
	public interface IDasbhoardInputService
	{
		Task<IReadOnlyCollection<DashboardInputDto>> GetDashboardInputAsync(DashboardInputFilterDto dashboardInputFilter);
        Task<IReadOnlyCollection<DashboardInputSummary>> GetDashboardInputAuditSummary();
        Task SaveDashboardInputValues(SaveDashboardInputValuesDto[] saveDashboardInputValuesDtos);
		Task<DashboardInputDto> AddTableToDashboardInput(AddTableDto addTableDto);
        Task<DashboardInputDto> EditTableToDashboardInput(EditDashboardInputDto editDashboardInputDto);
        Task<DashboardInputDto> DeleTableToDashboardInput(EditDashboardInputDto editDashboardInputDto);
        Task<DashboardInputDto> AddGroupToDashboardInput(AddGroupDto addGroupDto);
        Task<DashboardInputDto> EditGroupToDashboardInput(EditDashboardInputDto editDashboardInputDto);
        Task<DashboardInputDto> DeleteGroupToDashboardInput(EditDashboardInputDto editDashboardInputDto);
        Task<DashboardInputDto> AddElementToDashboardInput(AddElementDto addElementDto);
        Task<DashboardInputDto> EditElementToDashboardInput(EditDashboardInputDto editDashboardInputDto);
        Task<DashboardInputDto> DeleteElementToDashboardInput(EditDashboardInputDto editDashboardInputDto);
        Task<int> FillElementsWithData(int organizationId, DashboardInputElement dashboardInputElement, Dictionary<string, int> pairs);
        Task<DashboardInputElement> GetDashboardInputElement(int elementId);
        Task InitDashboardInputForOrganization(int organizationId);
    }
}

