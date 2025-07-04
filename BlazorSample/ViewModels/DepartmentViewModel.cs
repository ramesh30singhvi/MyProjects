using CellarPassAppAdmin.Shared.Entities.v4;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class DepartmentViewModel : IDepartmentViewModel
    {
        private IDepartmentService _departmentService;

        public DepartmentViewModel(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }
        public async Task<BusinessDepartmentListResponse> GetBusinessDepartmentListAsync(int businessId)
        {
            var result = await _departmentService.GetBusinessDepartmentListAsync(businessId);
            return result;
        }
        public async Task<BusinessDepartmentResponse> GetBusinessDepartmentByIdAsync(Guid id)
        {
            var result = await _departmentService.GetBusinessDepartmentByIdAsync(id);
            return result;
        }

        public async Task<BusinessDepartmentResponse> AddUpdateBusinessDepartmentAsync(BusinessDepartmentRequestModel businessDepartment)
        {
            var result = await _departmentService.AddUpdateBusinessDepartmentAsync(businessDepartment);
            return result;
        }
        public async Task<BusinessDepartmentListResponse> DeleteBusinessDepartmentByIdAsync(Guid id)
        {
            var result = await _departmentService.DeleteBusinessDepartmentByIdAsync(id);
            return result;
        }
    }
}
