using CellarPassAppAdmin.Shared.Entities.v4;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IDepartmentViewModel
    {
        Task<BusinessDepartmentListResponse> GetBusinessDepartmentListAsync(int businessId);
        Task<BusinessDepartmentResponse> GetBusinessDepartmentByIdAsync(Guid id);
        Task<BusinessDepartmentResponse> AddUpdateBusinessDepartmentAsync(BusinessDepartmentRequestModel businessDepartment);
        Task<BusinessDepartmentListResponse> DeleteBusinessDepartmentByIdAsync(Guid id);
    }
}
