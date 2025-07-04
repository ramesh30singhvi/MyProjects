using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Models;
using SHARP.DAL.Models.QueryModels;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IFormOrganizationRepository : IRepository<FormOrganization>
    {
        Task<FormOrganization[]> GetOrganizationFormsAsync(FormFilter filter, Expression<Func<FormOrganization, object>> orderBySelector);

        Task<IReadOnlyCollection<FilterOptionQueryModel>> GetDistinctColumnAsync(
            OrganizationFormFilterColumnSource<OrganizationFormFilterColumn> columnData, 
            Expression<Func<FormOrganization, FilterOptionQueryModel>> columnSelector);
    }
}
