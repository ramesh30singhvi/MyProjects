using System;
using System.Threading.Tasks;
using SHARP.DAL.Models;

namespace SHARP.DAL.Repositories.Interfaces
{
	public interface IFormGroupRepository : IRepository<FormGroup>
	{
        Task<FormGroup[]> GetGroupsAsync(params int[] formSectionIds);
        Task<FormGroup> GetFormGroupAsync(int id);
	}
}

